import styles from "../LoginPage/LoginPage.module.css";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showError, setShowError] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    if (
      localStorage.getItem("LoggedIn") != null &&
      localStorage.getItem("LoggedIn") === "true"
    ) {
      navigate("/main");
    }
  }, []);

  function handleUsernameInput(e) {
    setUsername(e.target.value);
  }
  function handlePasswordInput(e) {
    setPassword(e.target.value);
  }
  function handleSubmit() {
    const apiString =
      "http://localhost:5000/login/" + encodeURIComponent(username) + "/" + encodeURIComponent(password);
    fetch(apiString)
      .then((response) => response.text())
      .then((data) => {
        if (!isNaN(data) && data.trim() !== "") {
          localStorage.setItem("LoggedIn", "true");
          localStorage.setItem("Username", username);
          localStorage.setItem("CurrentRoom", "Main");
          localStorage.setItem("UserId", data);
          navigate("/main");
        } else if (data == "false") {
          setShowError(true);
        }
      })
      .catch((error) => console.error("Login API Error: " + error));
  }
  return (
      <div className={styles.pageWrapper}>
    <div className={styles.parent}>
      <label>Username</label>
      <input type="text" onInput={handleUsernameInput}></input>
      <label>Password</label>
      <input type="password" onInput={handlePasswordInput}></input>
      <button onClick={handleSubmit}>Login</button>
      {showError && (
        <div className={styles.errorMessage}>Invalid login credentials</div>
      )}
      <a className={styles.links} onClick={()=>navigate("/firstlogin")}>New User</a>
      <a className={styles.links} onClick={()=>navigate("/forgotpassword")}>Forgot Password</a>


    </div>
      </div>
  );
}

export default LoginPage;
