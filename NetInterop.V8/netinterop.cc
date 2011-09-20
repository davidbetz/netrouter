#ifdef _MSC_VER
#define _CRT_SECURE_NO_WARNINGS
#endif

#define MONITOR NetInterop::Routing::Monitor

#define _STRING System::String^
#define _INT32 System::Int32
#define _BOOLEAN System::Boolean
#define _TUPLE System::Tuple

#define $A Array::New
#define $S String::New
#define $OT ObjectTemplate::New

#define DECLARE_OUTPUT Local<String> result = $S("");
#define APPEND_TEXT(DATA) result = String::Concat(result, $S(DATA));
#define APPEND_COMMA result = String::Concat(result, $S(","));
#define APPEND_NEWLINE result = String::Concat(result, $S("\n"));
#define OUTPUT result

//#define CREATE_OBJECT(name) Local<Object> name =  nameTemplate->NewInstance();


#define SPRINTF(BUFFER, RESULT, FORMAT, DATA) sprintf(BUFFER, FORMAT, DATA); \
	RESULT = String::Concat(RESULT, $S(BUFFER))
#define CONCAT(RESULT, DATA) RESULT = String::Concat(RESULT, $S(DATA))
#define MVAL(DATA) (char*)(void*)Marshal::StringToHGlobalAnsi(DATA.ToString())
#define MREF(DATA) DATA == nullptr ? "" : (char*)(void*)Marshal::StringToHGlobalAnsi(DATA->ToString())

#pragma comment(lib, "..\\_REFERENCE\\node")

#include <node.h>
#include <v8.h>
#include <stdio.h>

#include <pcap.h>

void ifprint(pcap_if_t *d);
char *iptos(u_long in);
char* ip6tos(struct sockaddr *sockaddr, char *address, int addrlen);

using namespace v8;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

Handle<Value> GetDeviceIndexDictionary(const Arguments& args) {
	//HandleScope scope;
	//
	//Dictionary<_INT32, _STRING>^ dictionary = MONITOR::GetDeviceIndexDictionary();
 //  
	//DECLARE_OUTPUT
	//for each(_INT32 key in dictionary->Keys) {
	//	APPEND_TEXT(MVAL(key))
	//	APPEND_COMMA
	//	APPEND_TEXT(MREF(dictionary[key]))
	//	APPEND_NEWLINE
	//}
	//return scope.Close(OUTPUT);
	
	Dictionary<_INT32, _STRING>^ dictionary = MONITOR::GetDeviceIndexDictionary();

	HandleScope scope;
	Handle<ObjectTemplate> objectTemplate = $OT();
	Local<Array> result = $A(dictionary->Count);
	int i=0;
	for each(_INT32 key in dictionary->Keys) {
		Local<Object> instance =  objectTemplate->NewInstance();
		instance->Set($S("index"), $S(MVAL(key)));
		instance->Set($S("id"), $S(MREF(dictionary[key])));
		result->Set(i, instance);
		i++;
	}
	return scope.Close(result);
}


Handle<Value> GetDeviceDictionary(const Arguments& args) {
	Dictionary<_INT32, _TUPLE<_STRING,_STRING,_STRING, List<_TUPLE<_STRING, _STRING, _BOOLEAN>^>^>^>^ dictionary = MONITOR::GetDeviceDictionary();

	HandleScope scope;
	Handle<ObjectTemplate> outerDictionaryTemplate = $OT();
	Handle<ObjectTemplate> dataTemplate = $OT();
	Handle<ObjectTemplate> addressTemplate = $OT();
	Local<Array> result = $A(dictionary->Count);
	int i=0;
	for each(_INT32 key in dictionary->Keys) {
		_TUPLE<_STRING,_STRING,_STRING, List<_TUPLE<_STRING, _STRING, _BOOLEAN>^>^>^ inner = dictionary[key];
		
		Local<Array> addressArray = $A(inner->Item4->Count);
		int j=0;
		for each(_TUPLE<_STRING, _STRING, _BOOLEAN>^ addressData in inner->Item4) {
			Local<Object> address =  addressTemplate->NewInstance();
			address->Set($S("address"), $S(MREF(addressData->Item1)));
			address->Set($S("mask"), $S(MREF(addressData->Item2)));
			address->Set($S("isPrimary"), $S(MVAL(addressData->Item3)));
			addressArray->Set(j++, address);
		}

		Local<Object> data =  dataTemplate->NewInstance();
		data->Set($S("deviceID"), $S(MREF(inner->Item1)));
		data->Set($S("description"), $S(MREF(inner->Item2)));
		data->Set($S("mac"), $S(MREF(inner->Item3)));
		data->Set($S("addressList"), addressArray);

		Local<Object> outerDictionary =  outerDictionaryTemplate->NewInstance();
		outerDictionary->Set($S("index"), $S(MVAL(key)));
		outerDictionary->Set($S("data"), data);
		result->Set(i++, outerDictionary);
	}

	return scope.Close(result);
}

Handle<String> SerializeConfiguration(Nalarium::Configuration::ConfigurationSection^ section) {
    Newtonsoft::Json::Serialization::CamelCasePropertyNamesContractResolver^ resolver = gcnew Newtonsoft::Json::Serialization::CamelCasePropertyNamesContractResolver();
    Newtonsoft::Json::JsonSerializerSettings^ settings = gcnew Newtonsoft::Json::JsonSerializerSettings();
    settings->ContractResolver = resolver;
    return $S(MREF(Newtonsoft::Json::JsonConvert::SerializeObject(section, Newtonsoft::Json::Formatting::Indented, settings)));
}

Handle<Value> GetSystemConfig(const Arguments& args) {
	HandleScope scope;
	return scope.Close(SerializeConfiguration(NetInterop::Routing::Configuration::SystemSection::GetConfigSection()));
}

Handle<Value> GetRipConfig(const Arguments& args) {
	HandleScope scope;
	return scope.Close(SerializeConfiguration(NetInterop::Routing::Rip::Configuration::RipSection::GetConfigSection()));
}

Handle<Value> GetOspfConfig(const Arguments& args) {
	HandleScope scope;
	return scope.Close(SerializeConfiguration(NetInterop::Routing::Ospf::Configuration::OspfSection::GetConfigSection()));
}

//Handle<Value> GetDeviceDictionary(const Arguments& args) {
//	HandleScope scope;
//	char buffer[1000];
//
//	Dictionary<_INT32, Tuple<_STRING,_STRING,_STRING, List<Tuple<_STRING, _STRING, _BOOLEAN>^>^>^>^ dictionary = MONITOR::GetDeviceDictionary();
//   
//	DECLARE_OUTPUT
//	for each(_INT32 key in dictionary->Keys) {
//		APPEND_TEXT(MVAL(key))
//		APPEND_COMMA
//		APPEND_TEXT(MREF(dictionary[key]))
//		APPEND_NEWLINE
//	}
//	return scope.Close(OUTPUT);
//}


////Dictionary<Int32, _STRING> ^Monitor::GetDeviceIndexDictionary() {
//Handle<Value> GetDeviceIndexDictionary(const Arguments& args) {
//  HandleScope scope;
//	pcap_if_t *alldevs;
//	pcap_if_t *d;
//	char errbuf[PCAP_ERRBUF_SIZE];
//	int i=0;
//
//	if(pcap_findalldevs(&alldevs, errbuf) == -1)
//	{
//		OnVerboseOutput(String::Format("\Error in pcap_findalldevs: {0}\n", gcnew String(errbuf)));
//		exit(1);
//	}
//	
//	Dictionary<Int32, _STRING>^ data = gcnew Dictionary<Int32, _STRING>();
//	for(d=alldevs; d; d=d->next)
//	{
//		if (d->description)
//		{
//			data->Add(i++, gcnew String(d->name));
//		}
//	}
//  return scope.Close(result);
//}

extern "C" {
	void init(Handle<Object> target) {
		target->Set(String::NewSymbol("getDeviceIndexDictionary"), FunctionTemplate::New(GetDeviceIndexDictionary)->GetFunction());
		target->Set(String::NewSymbol("getDeviceDictionary"), FunctionTemplate::New(GetDeviceDictionary)->GetFunction());
		target->Set(String::NewSymbol("getSystemConfig"), FunctionTemplate::New(GetSystemConfig)->GetFunction());
		target->Set(String::NewSymbol("getRipConfig"), FunctionTemplate::New(GetRipConfig)->GetFunction());
		target->Set(String::NewSymbol("getOspfConfig"), FunctionTemplate::New(GetOspfConfig)->GetFunction());
	}
}

NODE_MODULE(netinterop, init)