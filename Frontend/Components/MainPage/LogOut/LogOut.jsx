import styles from "./LogOut.module.css";
import logoutDoor from "../../../Assets/exit.png";
import dashboard from "../../../Assets/dashboard.png";
import {useNavigate} from "react-router-dom";
import {useEffect, useState} from "react";

function LogOut({ username, onLogout, toggleRoomUserInfoModal }) {
    const navigate = useNavigate();

    const [isAdmin,setIsAdmin] = useState(false);

    useEffect(() => {
        fetch(`http://localhost:5000/isAdmin/${encodeURIComponent(localStorage.getItem("UserId"))}`).then(res=>res.json()).then(data=>{if(data==true)
        {
            setIsAdmin(true);
        }}).catch(
            err=>{console.error(err)}
        )
    }, []);

    function handleDashboardClick() {
        navigate("/admin");
   }

  return (
    <div className={styles.parent}>
      <div className={styles.username} onClick={()=>toggleRoomUserInfoModal()}>{username}</div>

        {isAdmin &&
            <img src={dashboard} className={styles.dashboard} alt="dashboard" onClick={() => handleDashboardClick()} />}
      <img
        src={logoutDoor}
        className={styles.logoutDoor}
        onClick={onLogout}
        alt="logout"
      />
    </div>
  );
}

export default LogOut;
