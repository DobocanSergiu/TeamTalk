import styles from "./MessageComponent.module.css";
function MessageComponent({ Username, MessageText }) {
  return (
    <li className={styles.parent}>
      <div className={styles.username}>{Username}</div>
      <div className={styles.message}>{MessageText} </div>
    </li>
  );
}
export default MessageComponent;
