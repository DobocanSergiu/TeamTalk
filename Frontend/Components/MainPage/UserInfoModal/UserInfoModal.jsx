import React, {useEffect, useState} from "react";
import styles from "../UserInfoModal/UserInfoModal.module.css";

function UserInfoModal({ isOpen, onClose }) {
  const [userTokens, setUserTokens] = useState([]);
  const [inputPassword, setInputPassword] = useState("");
  const [submitPasswordSuccess, setSubmitPasswordSuccess] = useState(false);
  const [submitPasswordFailure, setSubmitPasswordFailure] = useState(false);
  useEffect(() => {
    fetch(
      `http://localhost:5000/getTokens/${encodeURIComponent(localStorage.getItem("UserId"))}`,
    )
      .then((response) => {
        if (!response.ok) {
          throw new Error(`Failed to fetch tokens. Status: ${response.status}`);
        }
        return response.json();
      })
      .then((data) => {
        const tokens = data.tokens;
        setUserTokens(tokens);
      })
      .catch((error) => {
        console.error("Error fetching tokens:", error.message);
      });
  }, []);
  if (!isOpen) {
    return null;
  }

    return (
    <div className={styles.modalOverlay}>
      <div className={styles.modalContent}>
        <h1 className={styles.header}>Regen Tokens</h1>

        {userTokens.map((token) => {
          return (
            <code style={{ backgroundColor: "white" }}>
              Token: {token.token}
            </code>
          );
        })}

        <div className={styles.closeButtonContainer}>
          <button className={styles.closeButton} onClick={onClose}>
            Close
          </button>
        </div>
      </div>
    </div>
  );
}

export default UserInfoModal;
