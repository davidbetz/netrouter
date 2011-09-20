#include "stdafx.h"
#include "pcap.h"
#include <vcclr.h>
#include <iphlpapi.h>

#define IPTOSBUFFERS	12

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

void packet_handler(u_char *param, const struct pcap_pkthdr *header, const u_char *pkt_data);

char *iptos(u_long in)
{
	char output[IPTOSBUFFERS][3*4+3+1];
	short which;
	u_char *p;

	p = (u_char *)&in;
	which = (which + 1 == IPTOSBUFFERS ? 0 : which + 1);
	sprintf(output[which], "%d.%d.%d.%d", p[0], p[1], p[2], p[3]);
	return output[which];
}

namespace NetInterop {
	namespace Routing {
		public ref class Monitor {
		public :
			delegate void Write(String ^text);
			delegate void CallbackPlain();
			delegate void Callback(int len, const u_char *pkt_data);
			delegate void CallbackIntPtr(int len, IntPtr ptr);
			static Dictionary<Int32, String^> ^GetDeviceIndexDictionary();
			static Dictionary<Int32, Tuple<String^,String^,String^, List<Tuple<String^, String^, Boolean>^>^>^>^ GetDeviceDictionary();
			pcap_t *adhandle;
			int Initialize(int deviceNumber, String ^filter);
			void RunIt();
			void Stop();
			void Send(int size, u_char *packet);
			void Send(int size, IntPtr packet);

			static Write ^VerboseOutput;
			CallbackPlain ^BeforeCallback;
			CallbackPlain ^AfterCallback;
			Callback ^ActiveCallback;
		    CallbackIntPtr ^ActiveCallbackIntPtr;
			static Dictionary<Int32, CallbackIntPtr^>^ CallbackArray;
			static Monitor ^Create();
		private:
			int _index;
			static Monitor();
			static Monitor ^_monitor;
			static void OnVerboseOutput(String ^text);
		};

		Monitor ^Monitor::Create() {
			return gcnew Monitor();
		}

	    static Monitor::Monitor() {
			CallbackArray = gcnew Dictionary<Int32, CallbackIntPtr^>();
		}

		void Monitor::OnVerboseOutput(String ^text) {
			if(VerboseOutput) {
				VerboseOutput(text);
			}
		}
		
		void Monitor::Stop() {
			pcap_breakloop(adhandle);
		}

		void Monitor::Send(int size, u_char *packet) {
			if (pcap_sendpacket(adhandle, packet, size) != 0) {
				OnVerboseOutput(String::Format("\nError sending the packet: {0}\n", gcnew String(pcap_geterr(adhandle))));
			}
		}

		void Monitor::Send(int size, IntPtr packet) {
			Send(size, (u_char *)packet.ToPointer());
			//if (pcap_sendpacket(adhandle, packet, size) != 0) {
			//	OnVerboseOutput(String::Format("\nError sending the packet: {0}\n", gcnew String(pcap_geterr(adhandle))));
			//}
		}

		Dictionary<Int32, String^> ^Monitor::GetDeviceIndexDictionary() {
			pcap_if_t *alldevs;
			pcap_if_t *d;
			char errbuf[PCAP_ERRBUF_SIZE];
			int i=0;

			if(pcap_findalldevs(&alldevs, errbuf) == -1)
			{
				OnVerboseOutput(String::Format("\Error in pcap_findalldevs: {0}\n", gcnew String(errbuf)));
				exit(1);
			}
	
			Dictionary<Int32, String^>^ data = gcnew Dictionary<Int32, String^>();
			for(d=alldevs; d; d=d->next)
			{
				if (d->description)
				{
					data->Add(i++, gcnew String(d->name));
				}
			}
			return data;
		}

		Dictionary<Int32, Tuple<String^,String^,String^, List<Tuple<String^, String^, Boolean>^>^>^> ^Monitor::GetDeviceDictionary() {
			pcap_if_t *alldevs;
			pcap_if_t *d;
			char errbuf[PCAP_ERRBUF_SIZE];
			int i=0;

			if(pcap_findalldevs(&alldevs, errbuf) == -1)
			{
				OnVerboseOutput(String::Format("\Error in pcap_findalldevs: {0}\n", gcnew String(errbuf)));
				exit(1);
			}
			
			IP_ADAPTER_INFO *info = NULL, *pos;
			DWORD size = 0;
			GetAdaptersInfo(info, &size);

			info = (IP_ADAPTER_INFO *)malloc(size);

			GetAdaptersInfo(info, &size);
			System::Text::StringBuilder^ sb;
			Dictionary<Int32, Tuple<String^,String^,String^, List<Tuple<String^, String^, Boolean>^>^>^>^ data = gcnew Dictionary<Int32, Tuple<String^,String^,String^, List<Tuple<String^, String^, Boolean>^>^>^>();
			for (pos=info; pos!=NULL; pos=pos->Next) {
				sb = gcnew System::Text::StringBuilder();
				//Int32.Parse(type_or_length.segment1.ToString("00") + type_or_length.segment2.ToString("00"), NumberStyles.HexNumber)
				sb->Append(System::String::Format("{0}", pos->Address[0]));
				for (int j=1; j<pos->AddressLength; j++) {
					sb->Append(":");
					sb->Append(System::String::Format("{0}", pos->Address[j]));
				}
				IP_ADDR_STRING *ip;
				List<Tuple<String^, String^, Boolean>^>^ list = gcnew List<Tuple<String^, String^, Boolean>^>();
				list->Add(gcnew Tuple<String^, String^, Boolean>(gcnew String(pos->IpAddressList.IpAddress.String), gcnew String(pos->IpAddressList.IpMask.String), true));
				for (ip=pos->IpAddressList.Next; ip!=NULL; ip=ip->Next) {
					list->Add(gcnew Tuple<String^, String^, Boolean>(gcnew String(ip->IpAddress.String), gcnew String(ip->IpMask.String), false));
				}
				//for (int j=1; j<pos->IpAddressList; j++) {
				data->Add(i++, gcnew Tuple<String^,String^,String^, List<Tuple<String^, String^, Boolean>^>^>(gcnew String(pos->AdapterName), gcnew String(pos->Description), sb->ToString(), list));
			}

			free(info);

			return data;
		}

		int Monitor::Initialize(int deviceNumber, String ^filter) {
			_index = deviceNumber;
			char *pNewCharStr;
			int i=0;
			System::Threading::Monitor::Enter(CallbackArray);
			CallbackArray->Add(_index, ActiveCallbackIntPtr);
			System::Threading::Monitor::Exit(CallbackArray);
		
			pNewCharStr = (char*)(void*)Marshal::StringToHGlobalAnsi(filter);
	
			pcap_if_t *alldevs;
			pcap_if_t *d;
			char errbuf[PCAP_ERRBUF_SIZE];

			/* Retrieve the device list */
			if(pcap_findalldevs(&alldevs, errbuf) == -1)
			{
				OnVerboseOutput(String::Format("\Error in pcap_findalldevs: {0}\n", gcnew String(errbuf)));
				exit(1);
			}
			if(deviceNumber < 0)
			{
				OnVerboseOutput("\nInterface number out of range\n");
				pcap_freealldevs(alldevs);
				return -1;
			}
	
			for(d=alldevs, i=0; i< deviceNumber ;d=d->next, i++);
	
			if ((adhandle = pcap_open_live(d->name,
									 65536,		
									 1,			
									 1000,		
									 errbuf		
									 )) == NULL) {
				OnVerboseOutput(String::Format("\nUnable to open the adapter. {0} is not supported by WinPcap\n", gcnew String(d->name)));
				pcap_freealldevs(alldevs);
				return -1;
			}
	
			OnVerboseOutput(String::Format("\nlistening on {0}...\n", gcnew String(d->description)));
	
			pcap_freealldevs(alldevs);
	
			struct bpf_program fcode;
			bpf_u_int32 netmask;
			if (pNewCharStr != NULL)
			{
				if (d->addresses != NULL) {
					netmask = ((struct sockaddr_in *)(d->addresses->netmask))->sin_addr.S_un.S_addr;
				}
				else {
					netmask = 0xffffff; 
				}

				if(pcap_compile(adhandle, &fcode, pNewCharStr, 1, netmask) < 0)
				{
					OnVerboseOutput("\nError compiling filter: wrong syntax.\n");

					pcap_close(adhandle);
					return -3;
				}

				if(pcap_setfilter(adhandle, &fcode)<0)
				{
					OnVerboseOutput("\nError setting the filter\n");

					pcap_close(adhandle);
					return -4;
				}
			}
		}

		void Monitor::RunIt() {
			if(adhandle) {
				pcap_loop(adhandle, 0, packet_handler, (u_char*)_index);
				pcap_close(adhandle);
			}
			else {
				OnVerboseOutput("\Monitor not initialized.\n");
			}
		}
	}
}

void packet_handler(u_char *user, const struct pcap_pkthdr *header, const u_char *pkt_data)
{
	//struct tm *ltime;
	//char timestr[16];
	//time_t local_tv_sec;

	int _index = (int)user;
	System::Threading::Monitor::Enter(NetInterop::Routing::Monitor::CallbackArray);
	NetInterop::Routing::Monitor::CallbackArray[_index]((int)header->len, (IntPtr)(void*)pkt_data);
	System::Threading::Monitor::Exit(NetInterop::Routing::Monitor::CallbackArray);


	//(VOID)(param);

	//local_tv_sec = header->ts.tv_sec;
	//ltime=localtime(&local_tv_sec);
	//strftime( timestr, sizeof timestr, "%H:%M:%S", ltime);
	//printf("%s,%.6d len:%d", timestr, header->ts.tv_usec, header->len);
	
	//if(CallbackArray[_index]) {
	//	CallbackArray[_index]((int)header->len, (IntPtr)(void*)pkt_data);
	//}
	//if(BeforeCallback) {
	//	BeforeCallback();
	//}
	//if(ActiveCallbackIntPtr) {
	//	ActiveCallbackIntPtr((int)header->len, (IntPtr)(void*)pkt_data);
	//}
	//else if(ActiveCallback) {
	//	ActiveCallback((int)header->len, pkt_data);
	//}
	//if(AfterCallback) {
	//	AfterCallback();
	//}
}
