import { useEffect, useRef, useState } from "react";
import styles from "./ChatComponent.module.css";
import MessageComponent from "./MessageComponent";
import FileMessageComponent from "./FileMessageComponent";

function ChatComponent({ roomMessages, room, onSocketUpdate }) {
  const [messageInput, setMessageInput] = useState("");
  const [fileInput, setFileInput] = useState(null);
  const [fileBase64, setFileBase64] = useState(null);
  const [messages, setMessages] = useState([...roomMessages]);
  const socketRef = useRef(null);
  const lastMessageRef = useRef(null);

  useEffect(() => {
    setMessages(roomMessages);

    if (socketRef.current) {
      socketRef.current.close();
      socketRef.current = null;
    }

    const socket = new WebSocket(
      `ws://127.0.0.1:8080/Chat/${room}?name=${localStorage.getItem("UserId")}_${localStorage.getItem("Username")}`,
    );
    socketRef.current = socket;
    onSocketUpdate(socket);

    socket.onopen = () => {
      console.log("WebSocket connection established for room:", room);
    };

    socket.onmessage = (event) => {
      const receivedMessage = event.data;
      const jsonMessage = JSON.parse(receivedMessage);
      setMessages((prevMessages) => [...prevMessages, jsonMessage]);
    };

    return () => {
      if (socketRef.current) {
        socketRef.current.close();
      }
    };
  }, [roomMessages, room, onSocketUpdate]);

  function handleMessageInput(event) {
    setMessageInput(event.target.value);
  }

  function handleFileInput(event) {
    const selectedFile = event.target.files[0];
    setFileInput(selectedFile);

    const reader = new FileReader();
    reader.onload = () => {
      setFileBase64(reader.result);
    };
    reader.onerror = (error) => {
      console.error("Error reading file:", error);
    };

    reader.readAsDataURL(selectedFile);
  }

  const sendMessage = () => {
    if (messageInput !== "") {
      const textMessage = {
        isFile: false,
        size: new Blob([messageInput]).size,
        name: "message",
        type: "text",
        data: messageInput,
        sender: localStorage.getItem("Username"),
        senderId: localStorage.getItem("UserId"),
        time: new Date(),
      };
      socketRef.current.send(JSON.stringify(textMessage));
      setMessageInput("");
    }

    if (fileInput != null) {
      const fileMessage = {
        isFile: true,
        size: fileInput.size,
        name: fileInput.name,
        type: fileInput.type,
        data: fileBase64,
        sender: localStorage.getItem("Username"),
        senderId: localStorage.getItem("UserId"),
        time: new Date(),
      };
      socketRef.current.send(JSON.stringify(fileMessage));
      setFileInput(null);
    }
    setTimeout(() => {
      if (lastMessageRef.current) {
        lastMessageRef.current.scrollIntoView({ behavior: 'smooth' });
      }
    }, 100);
  };

  return (
    <div className={styles.parent}>
      <div className={styles.header}>Chat</div>
      <ul className={styles.chatContainer} >
        {messages.map((message, index) => {
          const isLastMessage = index === messages.length - 1;
          if (message.isFile === false) {
            return (
              <li key={index} ref={isLastMessage ? lastMessageRef : null}>
                <MessageComponent
                  Username={message.sender}
                  MessageText={message.data}
                />
              </li>
            );
          } else if (message.isFile === true) {
            return (
              <li key={index} ref={isLastMessage ? lastMessageRef : null}>
                <FileMessageComponent
                  Username={message.sender}
                  FileName={message.name}
                  FileType={message.type}
                  FileData={message.data}
                />
              </li>
            );
          }
          return null;
        })}
      </ul>
      <div style={{ display: "flex", flexDirection: "row" }}>
        <input
          className={styles.messageInput}
          value={messageInput}
          onChange={handleMessageInput}
          placeholder="Type a message..."
        />
        <label htmlFor="file-upload" className={styles.attachmentInput}>
          Upload
        </label>
        <input
          id="file-upload"
          type="file"
          onChange={handleFileInput}
          style={{ display: "none" }}
        />
        <button className={styles.sendButton} onClick={sendMessage}>
          Send
        </button>
        {fileInput && (
          <div className={styles.fileUploadedText}>{fileInput.name}</div>
        )}
      </div>
    </div>
  );
}

export default ChatComponent;
