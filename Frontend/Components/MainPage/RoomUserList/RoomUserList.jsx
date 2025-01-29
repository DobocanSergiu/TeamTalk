import styles from "./RoomUserList.module.css";
import profileIcon from "../../../Assets/profile.png";
import { useState } from "react";
function RoomUserList({ roomUsers }) {
  const [userList, setUserList] = useState([]);
  return (
    <div className={styles.parent}>
      <div className={styles.header}>Room Users</div>
      <hr className={styles.separator} />

      <ul className={styles.listContainer}>
        {roomUsers.map((roomUser) => {
          return (
            <li className={styles.listItem}>
              <img src={profileIcon} />
              <div>{roomUser}</div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}

export default RoomUserList;
