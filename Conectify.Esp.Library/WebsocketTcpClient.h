#ifndef WebsocketTcpClient_h
#define WebsocketTcpClient_h

#if defined(ARDUINO_ARCH_ESP8266)
#include <ESP8266WiFi.h>
#elif defined(ESP32)
#include <WiFi.h>
#else
#error Architecture unrecognized by this code.
#endif

#include <Arduino.h>
#include <ArduinoWebsockets.h>

// Network transport for the websocket client.
//
// The transport that comes with ArduinoWebsockets reads only what has arrived so far. When a frame header is split
// between two TCP packets, the client reads half of it and loses track of where frames begin. It then takes a payload
// length out of the message text and waits inside poll() for thousands of bytes that never come - the device hangs,
// while its connection stays open and the server keeps showing it as connected.
//
// This one reads exactly as much as asked for. When the rest does not arrive in time, or a frame cannot be written
// whole, it drops the connection, so the main loop connects again instead of hanging.
class WebsocketTcpClient : public websockets::network::TcpClient
{
public:
    bool connect(const websockets::WSString &host, const int port) override;
    bool poll() override;
    bool available() override;
    void send(const websockets::WSString &data) override;
    void send(const websockets::WSString &&data) override;
    void send(const uint8_t *data, const uint32_t len) override;
    websockets::WSString readLine() override;
    uint32_t read(uint8_t *buffer, const uint32_t len) override;
    void close() override;
    virtual ~WebsocketTcpClient();

protected:
    int getSocket() const override;

private:
    WiFiClient client;
    bool WaitForData(unsigned long lastProgress);
};

#endif
