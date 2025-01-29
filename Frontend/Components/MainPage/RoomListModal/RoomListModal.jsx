import React, { useEffect, useState } from "react";
import styles from "./RoomListModal.module.css";
import unknownIcon from "../../../Assets/unknown.png";
import wordIcon from "../../../Assets/word.png";
import excelIcon from "../../../Assets/excel.png";
import imageIcon from "../../../Assets/image.png";
import pdfIcon from "../../../Assets/pdf.png";
import powerPointIcon from "../../../Assets/powerpoint.png";
import textIcon from "../../../Assets/text.png";
import videoIcon from "../../../Assets/video.png";

function RoomListModal({ isOpen, onClose }) {
  const [rooms, setRooms] = useState([]);
  const [currentRoom, setCurrentRoom] = useState("Main");
  const [roomFiles, setRoomFiles] = useState([]);
  const [search, setSearch] = useState("");
  const [searchRoomFiles, setSearchRoomFiles] = useState([]);
  const [selectedFileType, setSelectedFileType] = useState("");
  const handleDownload = (fileName, fileData) => {
    const link = document.createElement("a");
    link.href = fileData;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  useEffect(() => {
    fetch(
      `http://localhost:5000/getUserRooms/${encodeURIComponent(localStorage.getItem("UserId"))}`,
    )
      .then((response) => response.json())
      .then((data) => {setRooms([...data])})
      .catch((e) => console.error("Error fetching rooms: " + e));

    fetch(`http://localhost:5000/getRoomFiles/${encodeURIComponent(currentRoom)}`)
        .then((response) => response.json())
        .then((data) => {setRoomFiles(data);setSearchRoomFiles(data);})
        .catch((e) => console.error("Error fetching files: " + e));


  }, [isOpen,]);

  useEffect(() => {
    if(search=="") {
      setSearchRoomFiles(roomFiles);
    }
    else
    {
      const filteredFiles = roomFiles.filter(roomFile =>
          roomFile.name.toLowerCase().includes(search.toLowerCase()));
      setSearchRoomFiles(filteredFiles);

    }


  }, [search]);

  useEffect(() => {

  }, [selectedFileType]);

  function handleRoomChange(room) {
    setCurrentRoom(room);
      fetch(`http://localhost:5000/getRoomFiles/${encodeURIComponent(room)}`)
        .then((response) => response.json())
        .then((data) => {setRoomFiles(data);setSearchRoomFiles(data);})
        .catch((e) => console.error("Error fetching files: " + e));

  }

  if (!isOpen) return null;
  return (
    <div className={styles.modalOverlay}>
      <div className={styles.modalContent}>
        <input
          type="text"
          placeholder="Search"
          onInput={(e) => setSearch(e.target.value)}
          className={styles.searchBar}
        ></input>
        <div className={styles.modalLists}>
          <div className={styles.roomListContainer}>
            <h1 className={styles.roomListHeader}>Room</h1>

            <ul className={styles.roomsList}>
              {rooms.map((room) => {
                if (currentRoom == room) {
                  return (
                    <li
                      className={styles.roomListItem}
                      style={{ backgroundColor: "orange" }}
                      onClick={() => handleRoomChange(room)}
                    >
                      {room}
                    </li>
                  );
                } else {
                  return (
                    <li
                      className={styles.roomListItem}
                      onClick={() => handleRoomChange(room)}
                    >
                      {room}
                    </li>
                  );
                }
              })}
            </ul>
          </div>
          <div className={styles.fileListContainer}>
            <h1 className={styles.fileListHeader}>Files</h1>
            <ul className={styles.fileList}>
              {searchRoomFiles.map((file) => {
                let icon;
                switch (file.type) {
                  case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    icon = wordIcon;
                    break;
                  case "application/msword":
                    icon = wordIcon;
                    break;
                  case "application/pdf":
                    icon = pdfIcon;
                    break;
                  case "image/png":
                    icon = imageIcon;
                    break;
                  case "image/jpeg":
                    icon = imageIcon;
                    break;
                  case "application/vnd.ms-excel":
                    icon = excelIcon;
                    break;
                  case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    icon = excelIcon;
                    break;
                  case "application/vnd.ms-powerpoint":
                    icon = powerPointIcon;
                    break;
                  case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    icon = powerPointIcon;
                    break;
                  case "text/plain":
                    icon = textIcon;
                    break;
                  case "video/mp4":
                    icon = videoIcon;
                    break;
                  case "video/webm":
                    icon = videoIcon;
                    break;
                  case "video/mpeg":
                    icon = videoIcon;
                    break;
                  default:
                    icon = unknownIcon;
                }

                return (
                  <li
                    className={styles.fileListItem}
                    onClick={() => handleDownload(file.name, file.data)}
                  >
                    <img src={icon} alt={`${file.type} icon`} />
                    <div>{file.name}</div>
                  </li>
                );
              })}
            </ul>
          </div>
        </div>
        <button onClick={onClose} className={styles.closeButton}>
          Close
        </button>
      </div>
    </div>
  );
}

export default RoomListModal;
