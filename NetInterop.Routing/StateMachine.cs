using System;
using Nalarium;

namespace NetInterop.Routing
{
    public class StateMachine<TState, TEvent>
        where TState : struct
        where TEvent : struct
    {
        private readonly Type _stateType;
        private readonly Map<TState, int> _valueIntMap;

        public StateMachine()
        {
            ActionMap = new Map<int, Map<TEvent, Tuple<NextStateMode, Action<TState, TEvent, object>, TState>>>();
            _stateType = typeof(TState);
            _valueIntMap = new Map<TState, int>();
            ActionMap.Add(0, new Map<TEvent, Tuple<NextStateMode, Action<TState, TEvent, object>, TState>>());
            foreach (object value in Enum.GetValues(_stateType))
            {
                _valueIntMap.Add((TState)value, (int)value);
                ActionMap.Add((int)value, new Map<TEvent, Tuple<NextStateMode, Action<TState, TEvent, object>, TState>>());
            }
        }

        public Map<int, Map<TEvent, Tuple<NextStateMode, Action<TState, TEvent, object>, TState>>> ActionMap { get; set; }

        public Map<TEvent, Tuple<NextStateMode, Action<TState, TEvent, object>, TState>> this[TState state]
        {
            get
            {
                return ActionMap[_valueIntMap[state]];
            }
            set
            {
                ActionMap[_valueIntMap[state]] = value;
            }
        }

        public void Add(TEvent evt, Action<TState, TEvent, object> action)
        {
            Add(default(TState), evt, action, StateEntryMode.Any);
        }

        public void Add(TEvent evt, Action<TState, TEvent, object> action, TState nextState)
        {
            Add(default(TState), evt, action, StateEntryMode.Any, NextStateMode.Defined, nextState);
        }

        public void Add(TState state, TEvent evt, Action<TState, TEvent, object> action)
        {
            Add(state, evt, action, StateEntryMode.Equals);
        }

        public void Add(TState state, TEvent evt, NextStateMode nextStateMode)
        {
            Add(state, evt, null, StateEntryMode.Equals, nextStateMode);
        }

        public void Add(TState state, TEvent evt, Action<TState, TEvent, object> action, TState nextState)
        {
            Add(state, evt, action, StateEntryMode.Equals, NextStateMode.Defined, nextState);
        }

        public void Add(TState state, TEvent evt, Action<TState, TEvent, object> action, StateEntryMode mode)
        {
            Add(state, evt, action, StateEntryMode.Equals, NextStateMode.Unknown, default(TState));
        }

        public void Add(TState state, TEvent evt, Action<TState, TEvent, object> action, StateEntryMode mode, TState nextState)
        {
            Add(state, evt, action, mode, NextStateMode.Defined, nextState);
        }

        public void Add(TState state, TEvent evt, StateEntryMode mode, NextStateMode nextStateMode)
        {
            Add(state, evt, (s, e, d) => { }, mode, nextStateMode);
        }

        public void Add(TState state, TEvent evt, Action<TState, TEvent, object> action, StateEntryMode mode, NextStateMode nextStateMode)
        {
            //++ if its not set, it's not defined, so it's not needed to save
            Add(state, evt, action, mode, nextStateMode, default(TState));
        }

        public void Add(TState state, TEvent evt, Action<TState, TEvent, object> action, StateEntryMode mode, NextStateMode nextStateMode, TState nextState)
        {
            if (mode == StateEntryMode.Any)
            {
                foreach (int key in ActionMap.Keys)
                {
                    ActionMap[key].Add(evt, new Tuple<NextStateMode, Action<TState, TEvent, object>, TState>(nextStateMode, action, nextState));
                }
            }
            else if (mode == StateEntryMode.AtLeast)
            {
                int bottom = _valueIntMap[state];
                foreach (int value in _valueIntMap.Values)
                {
                    if (value < bottom)
                    {
                        continue;
                    }
                    ActionMap[value].Add(evt, new Tuple<NextStateMode, Action<TState, TEvent, object>, TState>(nextStateMode, action, nextState));
                }
            }
            else
            {
                ActionMap[_valueIntMap[state]].Add(evt, new Tuple<NextStateMode, Action<TState, TEvent, object>, TState>(nextStateMode, action, nextState));
            }
        }

        public void RaiseEvent(TState state, TEvent evt)
        {
            RaiseEvent(state, evt, null);
        }

        public void RaiseEvent(TState state, TEvent evt, object data)
        {
            Map<TEvent, Tuple<NextStateMode, Action<TState, TEvent, object>, TState>> eventMap = ActionMap[_valueIntMap[state]];
            if (!eventMap.ContainsKey(evt))
            {
                throw new InvalidOperationException("event not found");
            }
            Tuple<NextStateMode, Action<TState, TEvent, object>, TState> actionData = eventMap[evt];
            if (actionData.Item1 != NextStateMode.NoAction)
            {
                actionData.Item2(state, evt, data);
            }
        }

        public bool GetNext(TState state, TEvent evt, out TState nextState)
        {
            Map<TEvent, Tuple<NextStateMode, Action<TState, TEvent, object>, TState>> eventMap = ActionMap[_valueIntMap[state]];
            if (!eventMap.ContainsKey(evt))
            {
                throw new InvalidOperationException("event not found");
            }
            Tuple<NextStateMode, Action<TState, TEvent, object>, TState> data = eventMap[evt];
            if (data.Item1 == NextStateMode.Defined)
            {
                nextState = data.Item3;
                return true;
            }
            nextState = default(TState);
            return false;
        }
    }
}