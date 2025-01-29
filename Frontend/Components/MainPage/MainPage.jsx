// MainPage.js
import RoomList from "./RoomList/RoomList";
import RoomDrive from "./RoomDrive/RoomDrive";
import ChatComponent from "./ChatComponent/ChatComponent";
import RoomUserList from "./RoomUserList/RoomUserList";
import styles from "./MainPage.module.css";
import { useEffect, useState, useRef } from "react";
import LogOut from "./LogOut/LogOut";
import { useNavigate } from "react-router-dom";
import RoomListModal from "./RoomListModal/RoomListModal";
import UserInfoModal from "./UserInfoModal/UserInfoModal";

function MainPage() {
  const [rooms, setRooms] = useState([]);
  const [currentRoom, setCurrentRoom] = useState("Main");
  const [roomUsers, setRoomUsers] = useState([]);
  const [roomFiles, setRoomFiles] = useState([]);
  const [roomMessages, setRoomMessages] = useState([]);
  const [roomListModalIsOpen, setRoomListModalIsOpen] = useState(false);
  const [userInfoModalIsOpen, setUserInfoModalIsOpen] = useState(false);
  const navigate = useNavigate();
  const socketRef = useRef(null);

  function toggleRoomListModal() {
    setRoomListModalIsOpen(!roomListModalIsOpen);
  }
  function toggleRoomUserInfoModal() {
    setUserInfoModalIsOpen(!userInfoModalIsOpen);

  }

  function handleRoomClick(newRoom) {
    setCurrentRoom(newRoom);
  }

  const handleSocketRef = (socket) => {
    socketRef.current = socket;
  };

  const handleLogout = () => {
    if (socketRef.current) {
      socketRef.current.close();
    }
    localStorage.removeItem("LoggedIn");
    localStorage.removeItem("UserId");
    localStorage.removeItem("Username");
    localStorage.removeItem("CurrentRoom");
    navigate("/login");
  };

  useEffect(() => {
    if (
      localStorage.getItem("LoggedIn") == null ||
      localStorage.getItem("LoggedIn") === "false"
    ) {
      navigate("/login");
    }
    fetch(`http://localhost:5000/userExists/${encodeURIComponent(localStorage.getItem("UserId"))}`).then(response=>{
      if(response.ok==false)
      {
        localStorage.removeItem("LoggedIn");
        localStorage.removeItem("UserId");
        localStorage.removeItem("Username");
        localStorage.removeItem("CurrentRoom");
        navigate("/login");

      }
    });
    fetch(
      `http://localhost:5000/getUserRooms/${encodeURIComponent(localStorage.getItem("UserId"))}`,
    )
      .then((response) => response.json())
      .then((data) => setRooms(data))
      .catch((e) => console.error("Error fetching rooms: " + e));

    fetch(`http://localhost:5000/getRoomUsers/${encodeURIComponent(currentRoom)}`)
      .then((response) => response.json())
      .then((data) => setRoomUsers(data))
      .catch((e) => console.error("Error fetching room users: " + e));

    fetch(`http://localhost:5000/getRoomFiles/${encodeURIComponent(currentRoom)}`)
      .then((response) => response.json())
      .then((data) => setRoomFiles(data))
      .catch((e) => console.error("Error fetching room files: " + e));

    fetch(`http://localhost:5000/getRoomMessages/${encodeURIComponent(currentRoom)}`)
      .then((response) => response.json())
      .then((data) => setRoomMessages(data))
      .catch((e) => console.error("Error fetching room messages: " + e));
  }, []);

  useEffect(() => {
    fetch(`http://localhost:5000/getRoomUsers/${encodeURIComponent(currentRoom)}`)
      .then((response) => response.json())
      .then((data) => setRoomUsers(data))
      .catch((e) => console.error("Error fetching room users: " + e));

    fetch(`http://localhost:5000/getRoomFiles/${encodeURIComponent(currentRoom)}`)
      .then((response) => response.json())
      .then((data) => setRoomFiles(data))
      .catch((e) => console.error("Error fetching room files: " + e));

    fetch(`http://localhost:5000/getRoomMessages/${encodeURIComponent(currentRoom)}`)
      .then((response) => response.json())
      .then((data) => setRoomMessages(data))
      .catch((e) => console.error("Error fetching room messages: " + e));
  }, [currentRoom]);

  return (
    <div className={styles.parent}>
      <div style={{ width: "15%", height: "95%" }}>
        <RoomList
          rooms={rooms}
          currentRoom={currentRoom}
          onRoomClick={handleRoomClick}
        />
        <LogOut
          username={localStorage.getItem("Username")}
          onLogout={handleLogout}
          toggleRoomUserInfoModal={toggleRoomUserInfoModal}

        />
      </div>
      <ChatComponent
        roomMessages={roomMessages}
        room={currentRoom}
        onSocketUpdate={handleSocketRef}
      />
      <div className={styles.rightSideComponent}>
        <RoomDrive
          roomFiles={roomFiles}
          toggleRoomListModal={toggleRoomListModal}
        />
        <RoomUserList roomUsers={roomUsers} />
      </div>
      <RoomListModal
        isOpen={roomListModalIsOpen}
        onClose={toggleRoomListModal}
      />
      <UserInfoModal
      isOpen={userInfoModalIsOpen}
      onClose={toggleRoomUserInfoModal}

      />
    </div>
  );
}

export default MainPage;
