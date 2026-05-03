#ifndef JACKBRIDGE_H
#define JACKBRIDGE_H

#include <windows.h>

#ifdef JACKBRIDGE_EXPORTS
#define JACKBRIDGE_API __declspec(dllexport)
#else
#define JACKBRIDGE_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef void (*LogCallback)(const char* message);
typedef void (*ConnectionCallback)(const char* process_name, DWORD pid, const char* dest_ip, UINT16 dest_port, const char* proxy_info);

typedef enum {
    PROXY_TYPE_HTTP = 0,
    PROXY_TYPE_SOCKS5 = 1
} ProxyType;

typedef enum {
    RULE_ACTION_PROXY = 0,
    RULE_ACTION_DIRECT = 1,
    RULE_ACTION_BLOCK = 2
} RuleAction;

typedef enum {
    RULE_PROTOCOL_TCP = 0,
    RULE_PROTOCOL_UDP = 1,
    RULE_PROTOCOL_BOTH = 2
} RuleProtocol;

JACKBRIDGE_API UINT32 JackBridge_AddRule(const char* process_name, const char* target_hosts, const char* target_ports, RuleProtocol protocol, RuleAction action);
JACKBRIDGE_API BOOL JackBridge_EnableRule(UINT32 rule_id);
JACKBRIDGE_API BOOL JackBridge_DisableRule(UINT32 rule_id);
JACKBRIDGE_API BOOL JackBridge_DeleteRule(UINT32 rule_id);
JACKBRIDGE_API BOOL JackBridge_EditRule(UINT32 rule_id, const char* process_name, const char* target_hosts, const char* target_ports, RuleProtocol protocol, RuleAction action);
JACKBRIDGE_API BOOL JackBridge_MoveRuleToPosition(UINT32 rule_id, UINT32 new_position);  // Move rule to specific position (1=first, 2=second, etc)
JACKBRIDGE_API UINT32 JackBridge_GetRulePosition(UINT32 rule_id);  // Get current position of rule in list (1-based)
JACKBRIDGE_API BOOL JackBridge_SetProxyConfig(ProxyType type, const char* proxy_ip, UINT16 proxy_port, const char* username, const char* password);  // proxy_ip can be IP address or hostname
JACKBRIDGE_API void JackBridge_SetDnsViaProxy(BOOL enable);
JACKBRIDGE_API void JackBridge_SetLocalhostViaProxy(BOOL enable);
JACKBRIDGE_API void JackBridge_SetLogCallback(LogCallback callback);
JACKBRIDGE_API void JackBridge_SetConnectionCallback(ConnectionCallback callback);
JACKBRIDGE_API void JackBridge_SetTrafficLoggingEnabled(BOOL enable);
JACKBRIDGE_API void JackBridge_ClearConnectionLogs(void);  // Clear connection history from memory
JACKBRIDGE_API BOOL JackBridge_Start(void);
JACKBRIDGE_API BOOL JackBridge_Stop(void);
JACKBRIDGE_API int JackBridge_TestConnection(const char* target_host, UINT16 target_port, char* result_buffer, size_t buffer_size);

#ifdef __cplusplus
}
#endif

#endif
