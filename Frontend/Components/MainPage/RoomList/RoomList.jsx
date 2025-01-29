import styles from "./RoomList.module.css";
function RoomList({ rooms, currentRoom, onRoomClick }) {
  return (
    <div className={styles.parent}>
      <div className={styles.header}>Room List</div>
      <hr className={styles.separator} />
      <ul className={styles.listContainer}>
        {rooms.map((room) => {
          if (room === currentRoom) {
            return (
              <li
                className={styles.listItem}
                style={{ background: "orange" }}
                onClick={() => onRoomClick(room)}
              >
                {room}
              </li>
            );
          } else {
            return (
              <li className={styles.listItem} onClick={() => onRoomClick(room)}>
                {room}
              </li>
            );
          }
        })}
      </ul>
    </div>
  );
}

export default RoomList;
