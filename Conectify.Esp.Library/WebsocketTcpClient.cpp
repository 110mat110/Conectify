#include "WebsocketTcpClient.h"
#include "DebugMessageLib.h"

// how long the rest of something that started arriving may take, before the connection is considered broken
const unsigned long WebsocketReadTimeoutMs = 5000;

bool WebsocketTcpClient::connect(const websockets::WSString &host, const int port)
{
  yield();
  bool connected = client.connect(host.c_str(), port);
  client.setNoDelay(true);
  return connected;
}

bool WebsocketTcpClient::poll()
{
  yield();
  return client.available() > 0;
}

bool WebsocketTcpClient::available()
{
  return client.connected();
}

void WebsocketTcpClient::send(const websockets::WSString &data)
{
  send(reinterpret_cast<const uint8_t *>(data.c_str()), data.size());
}

void WebsocketTcpClient::send(const websockets::WSString &&data)
{
  send(reinterpret_cast<const uint8_t *>(data.c_str()), data.size());
}

void WebsocketTcpClient::send(const uint8_t *data, const uint32_t len)
{
  yield();
  size_t written = client.write(data, len);
  if (written != len)
  {
    // the server would read whatever comes next as the rest of this frame
    DebugMessage("Websocket frame written only partially (" + String(written) + "/" + String(len) + "). Dropping the connection");
    client.stop();
  }
  yield();
}

websockets::WSString WebsocketTcpClient::readLine()
{
  websockets::WSString line = "";
  unsigned long lastProgress = millis();
  while (client.connected())
  {
    int ch = client.read();
    if (ch < 0)
    {
      if (!WaitForData(lastProgress))
      {
        return "";
      }
      continue;
    }

    lastProgress = millis();
    line += (char)ch;
    if (ch == '\n')
    {
      break;
    }
  }
  return line;
}

uint32_t WebsocketTcpClient::read(uint8_t *buffer, const uint32_t len)
{
  uint32_t done = 0;
  unsigned long lastProgress = millis();
  while (done < len && client.connected())
  {
    int received = client.read(buffer + done, len - done);
    if (received > 0)
    {
      done += received;
      lastProgress = millis();
    }
    else if (!WaitForData(lastProgress))
    {
      break;
    }
  }
  return done;
}

// Lets the network catch up for a moment. Once nothing has come for too long, drops the connection and gives up.
bool WebsocketTcpClient::WaitForData(unsigned long lastProgress)
{
  if (millis() - lastProgress > WebsocketReadTimeoutMs)
  {
    DebugMessage("Websocket data did not arrive in time. Dropping the connection");
    client.stop();
    return false;
  }
  delay(1);
  return true;
}

void WebsocketTcpClient::close()
{
  yield();
  client.stop();
}

WebsocketTcpClient::~WebsocketTcpClient()
{
  client.stop();
}

int WebsocketTcpClient::getSocket() const
{
  return -1;
}
