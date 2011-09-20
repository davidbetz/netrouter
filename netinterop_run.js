usingV8Standalone = true;
var __$moduleName = 'Debug/netinterop';
var globalWinpcap = require('./' + __$moduleName);

var _ = require("./_REFERENCE/underscore")
_.mixin(require("./_REFERENCE/underscore.string"));

var output = function (text) {
    console.log(text);
};

var dump = function (obj) {
    new trace.buffer().write(obj).flush();
};

var trace = require('../Nalarium-Trace/Nalarium.Trace');
trace.enable();

var Net = Net || {};

Net.RoutingController = (function () {
    //- ctor -//
    function ctor(init) {
        this._winpcap = require('./' + __$moduleName)
        this._deviceConfigurationMap = [];
    }
    ctor.prototype = {
        //- enable -//

        enable: function (filter) {
            // var wh = new AutoResetEvent(false);
            var _monitorList = [];
            output('Starting monitor...' + '\n');
            var hasInterfaces = this.setupDeviceInformation();
            if (hasInterfaces === false) {
                throw 'Nothing to do. No interfaces configured.';
            }
            // _mainThreadList = new List<Thread>();
            // output(this._deviceConfigurationMap);
            for (var key in this._deviceConfigurationMap) {
                var device = this._deviceConfigurationMap[key];
                if (device.isLoopback() === true) {
                    continue;
                }
                /*
                Monitor monitor = Monitor.Create();
                string copy = key;
                monitor.ActiveCallbackIntPtr = (s, p) => Begin(copy, s, p);
                //monitor.Initialize(3, filter);
                monitor.Initialize(DeviceConfigurationMap[key].Index, filter);
                _monitorList.Add(monitor);

                _mainThreadList.Add(new Thread(() =>
                {
                output(string.Format('Monitor started for {0}.' + '\n', device.ID));
                monitor.RunIt();
                output(string.Format('Monitor stopped for {0}.' + '\n', device.ID));
                }));
                */
            }
            /*
            Monitor.output += (t) => output(t);

            foreach (string info in ModuleTrackingInformationMap.Keys)
            {
            ModuleTrackingInformationMap[info].Module.Start();
            }

            IsEnabled = true;
            _mainThreadList.ForEach(p => p.Start());
            _timer = new Timer
            {
            Interval = 1000
            };
            _timer.Elapsed += (s, e) =>
            {
            lock (threadLock)
            {
            if (IsEnabled)
            {
            return;
            }
            output('Stopping monitor...' + '\n');
            _monitorList.ForEach(p => p.Stop());
            _timer.Stop();
            wh.Set();
            }
            };
            _timer.Start();
            wh.WaitOne();*/
        },


        setupDeviceInformation: function () {
            var deviceList = Net.RoutingController.getDeviceDictionary();
            var deviceIndexList = Net.RoutingController.getDeviceIndexDictionary();
            var getDeviceIndexByID = function (id) {
                for (var n in deviceIndexList) {
                    var entry = deviceIndexList[n];
                    if (entry.id.indexOf(id) > -1) {
                        return entry.index;
                    }
                }
                return -1;
            };
            var getPrimaryIPConfiguration = function (device) {
                var list = device.data.addressList;
                for (var n in list) {
                    var entry = list[n];
                    if (!!entry.isPrimary === true) {
                        return {
                            address: entry.address,
                            mask: entry.mask
                        };
                    }
                    return {};
                }
            };
            var getSecondaryIPConfigurationList = function (device) {
                var list = device.data.addressList;
                var returnList = [];
                for (var n in list) {
                    var entry = list[n];
                    if (!!entry.isPrimary === false) {
                        returnList.push({
                            address: entry.address,
                            mask: entry.mask
                        });
                    }
                }
                return returnList;
            };
            var foundDeviceList = [];
            var hasInterfaces = false;
            for (var key in deviceList) {
                var device = deviceList[key];
                //var deviceData = 
                /*var a = new trace.buffer();
                a.write(device.data);
                a.flush();*/
                //output(deviceList[key].data.deviceID);
                var deviceID = deviceList[key].data.deviceID.replace('{', '').replace('}', '');
                /*
                if (!SystemSection.GetConfigSection().Interfaces.Any(p => p.ID.Replace('{', string.Empty).Replace('}', string.Empty).Equals(deviceID)))
                {
                continue;
                }
                */
                foundDeviceList.push(deviceID);
                hasInterfaces = true;
                var list = getSecondaryIPConfigurationList(device);
                var v6List = []; // List<IPv6Configuration>();
                this._deviceConfigurationMap.push({
                    id: deviceID,
                    index: getDeviceIndexByID(deviceID),
                    description: device.data.description,
                    primaryIPConfiguration: getPrimaryIPConfiguration(device),
                    macAddress: device.data.mac,
                    secondaryIPList: list,
                    //TODO: do this
                    IPv6List: v6List,
                    isLoopback: function () { return false; }
                });
            }
            // output('==this._deviceConfigurationMap==');
            // new trace.buffer().write(this._deviceConfigurationMap).flush();
            var foundPrimary = false;
            //TODO: check for overlapping networks
            //IEnumerable<InterfaceElement> interfaceData = SystemSection.GetConfigSection().Interfaces.Where(p => !DeviceConfigurationMap.ContainsKey(p.ID));
            /*
            foreach (InterfaceElement interfaceElement in SystemSection.GetConfigSection().Interfaces)
            {
            string deviceID = interfaceElement.ID;
            if (foundDeviceList.Contains(deviceID))
            {
            DeviceConfigurationMap[deviceID].IsEnabled = !interfaceElement.IsDisabled;
            continue;
            }
            hasInterfaces = true;
            InterfaceElement element = interfaceElement;
            var device = new Device();
            DeviceConfigurationMap.Add(deviceID, device);
            foundDeviceList.Add(deviceID);
            foreach (AddressElement address in SystemSection.GetConfigSection().Addresses.Where(p => p.ID == element.ID))
            {
            var list = new List<IPConfiguration>();
            var listv6 = new List<IPv6Configuration>();
            device = new Device
            {
            Index = address.Index,
            Description = address.Name,
            PrimaryIPConfiguration = IPConfiguration.Create(
            ip_address.From(address.IP),
            ip_address.From(address.Mask)
            ),
            MacAddress = mac_address.From(address.Mac),
            SecondaryIPList = list,
            IPv6List = listv6
            };
            list.AddRange(address.Secondaries.Select(secondary => new IPConfiguration
            {
            Address = ip_address.From(secondary.Address),
            Mask = ip_address.From(secondary.Mask)
            }));
            listv6.AddRange(address.IPv6Addresses.Select(v6 => new IPv6Configuration
            {
            Address = ipv6_address.From(v6.Address),
            Mask = v6.Mask
            }));
            }
            }
            */
            if (!hasInterfaces) {
                return false;
            }
            /*
            //TODO: check for overlapping networks
            foreach (LoopbackElement interfaceElement in SystemSection.GetConfigSection().Loopbacks)
            {
            var list = new List<IPConfiguration>();
            var v6List = new List<IPv6Configuration>();
            var device = new Device
            {
            ID = interfaceElement.ID.ToString(),
            Index = interfaceElement.ID,
            PrimaryIPConfiguration = IPConfiguration.Create(
            ip_address.From(interfaceElement.IP),
            ip_address.From(interfaceElement.Mask)
            ),
            SecondaryIPList = list,
            IPv6List = v6List,
            Description = 'Loopback',
            IsLoopback = true,
            IsEnabled = true
            };
            LoopbackMap.Add(device.ID, device);
            DeviceConfigurationMap.Add(device.ID, device);
            foundDeviceList.Add(device.ID);
            list.AddRange(interfaceElement.Secondaries.Select(secondary => new IPConfiguration
            {
            Address = ip_address.From(secondary.Address),
            Mask = ip_address.From(secondary.Mask)
            }));
            v6List.AddRange(interfaceElement.IPv6Addresses.Select(v6 => new IPv6Configuration
            {
            Address = ipv6_address.From(v6.Address),
            Mask = v6.Mask
            }));
            }
            */
            return true;
        }

        /*
    
        enable: function(filter) {
        var deviceArray = this.getDeviceArray();
        output(deviceArray.length);
        for(var device in deviceArray) {
        if(deviceArray[device].isLoopback() === true) {
        continue;
        }
        output('go for ' + deviceArray[device].interfaceId);
        }
        },
        
        //- getDeviceArray -//
        getDeviceArray: function() {
        return [{
        isLoopback: function() { return false; },
        interfaceId: 'abc'
        },
        {
        isLoopback: function() { return true; },
        interfaceId: 'def'
        }
        ];
        }*/
    };
    //+
    return ctor;
})();

Net.RoutingController._cleanConfig = function (branch) {
    for (var n in branch) {
        if (n == "xmlns" || n == "sectionInformation" || n == "lockAttributes" ||
            n == "lockAllAttributesExcept" || n == "lockElements" || n == "lockAllElementsExcept" ||
            n == "lockItem" || n == "elementInformation" || n == "currentConfiguration") {
            delete branch[n];
        }
        else {
        }
    }
    for (var n in branch) {
        if (typeof branch[n] == 'object') {
            Net.RoutingController._cleanConfig(branch[n]);
        }
    }
}

Net.RoutingController.getSystemConfig = function () {
    eval('var config=' + globalWinpcap.getSystemConfig());
    Net.RoutingController._cleanConfig(config);
    return config;
};

Net.RoutingController.getRipConfig = function () {
    eval('var config=' + globalWinpcap.getRipConfig());
    Net.RoutingController._cleanConfig(config);
    return config;
};

Net.RoutingController.getOspfConfig = function () {
    eval('var config=' + globalWinpcap.getOspfConfig());
    Net.RoutingController._cleanConfig(config);
    return config;
};

Net.RoutingController.getDeviceIndexDictionary = function() {
    var winpcap = require('./' + __$moduleName);
    var deviceIndexDictionary = winpcap.getDeviceIndexDictionary();
    return deviceIndexDictionary;
};

Net.RoutingController.getDeviceDictionary = function() {
   var winpcap = require('./' + __$moduleName);
   var deviceDictionary = winpcap.getDeviceDictionary();
return deviceDictionary;
};

var controller = new Net.RoutingController();
//+
/*
var deviceIndexArray = Net.RoutingController.getDeviceIndexDictionary();
output('==deviceIndexArray==');
dump(deviceIndexArray);
//+
var deviceArray = Net.RoutingController.getDeviceDictionary();
output('==deviceArray==');
dump(deviceArray);
/*
_.each(deviceArray, function (item) {
    output(item.data.deviceID);
});
*/
//+
var systemConfig = Net.RoutingController.getSystemConfig();
var ripConfig = Net.RoutingController.getRipConfig();
var ospfConfig = Net.RoutingController.getOspfConfig();
output(systemConfig);
output(ripConfig);
output(ospfConfig);
controller.enable('tcp port eq 21');