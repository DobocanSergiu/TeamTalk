import styles from '../FirstLoginPage/FirstLoginPage.module.css'
import {useState} from "react";
import { useNavigate} from "react-router-dom";

function FirstLoginPage() {
    const [showError, setShowError] = useState(false);
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const navigate = useNavigate();

    function handleLoginButton()
    {

        const requestOptions = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
        };

        fetch(`http://localhost:5000/changePasswordNewUser/${username}/${password}`,requestOptions).then(res =>{
            if(res.ok)
            {
                setShowError(false);
                navigate("/login");

            }
            else
            {
                setShowError(true);
                throw new Error("Invalid username given");
            }
        }).catch(err=>{console.log(err)});
    }
    return (<div className={styles.pageWrapper}>
            <div className={styles.parent}>
                <label>Username</label>
                <input type="text"  onChange={(e)=>setUsername(e.target.value)}></input>
                <label>New Password</label>
                <input type="password" onChange={(e)=>setPassword(e.target.value)}></input>
                <button onClick={()=>handleLoginButton()} >Confirm</button>
                {showError && (
                    <div className={styles.errorMessage}>Invalid user</div>
                )}


            </div>
        </div>
    )

}

export default FirstLoginPage;