import styles from "../ForgotPasswordPage/ForgotPasswordPage.module.css";
import {useState} from "react";
import {useNavigate} from "react-router-dom";

function ForgotPasswordPage() {
    const [username,setUsername] = useState("");
    const [password,setPassword] = useState("");
    const [token,setToken] = useState("");
    const [showError, setShowError] = useState(false);
    const navigate = useNavigate();

    function handleForgotPasswordConfirm() {
        const requestOptions = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
        };


        fetch(`http://localhost:5000/forgotPassword/${encodeURIComponent(username)}/${encodeURIComponent(password)}/${encodeURIComponent(token)}`, requestOptions)
            .then(res =>{
                if(res.ok)
                {
                    setShowError(false);
                    navigate("/login")

                }
                else
                {
                    setShowError(true);


                }
            })
            .catch(err=>{console.log(err)})

    }

    return (<div className={styles.pageWrapper}>
        <div className={styles.parent}>
            <label>Username</label>
            <input type="text" onChange={(e)=>setUsername(e.target.value)}></input>
            <label>New Password</label>
            <input type="password" onChange={(e)=>setPassword(e.target.value)}></input>
            <label>Token</label>
            <input type="text" onChange={(e)=>setToken(e.target.value)}></input>
            <button onClick={()=>handleForgotPasswordConfirm()} >Confirm</button>
            {showError && (
                <div className={styles.errorMessage}>An error occured</div>
            )}


        </div>
    </div>)

}

export default ForgotPasswordPage;