import styles from "../AdminPage/AdminPage.module.css";
import {useEffect, useState} from "react";
function AdminPage() {
  const [rooms, setRooms] = useState([]);
  const [users, setUsers] = useState([]);
  const [addRoomName, setAddRoomName] = useState("");
  const [addRoomError, setAddRoomError] = useState(false);
  const [addRoomSuccess, setAddRoomSuccess] = useState(false);

  const [deleteRoom, setSelectedDeleteRoom] = useState("Main");
  const [deleteRoomError, setDeleteRoomError] = useState(false);
  const [deleteRoomSuccess, setDeleteRoomSuccess] = useState(false);

  const [createUser, setCreateUser] = useState();
  const [createUserError, setCreateUserError] = useState(false);
  const [createUserSuccess, setCreateUserSuccess] = useState(false);

  const [deleteUserMessage, setDeleteUserMessage] = useState(false);
  const [deleteUserId, setDeleteUserId] = useState("");
  const [deleteUserError, setDeleteUserError] = useState(false);
  const [deleteUserSuccess, setDeleteUserSuccess] = useState(false);

  const [addUserId, setAddUserId] = useState("");
  const [addUserRoom, setAddUserRoom] = useState("Main");
  const [addUserError, setAddUserError] = useState(false);
  const [addUserSuccess, setAddUserSuccess] = useState(false);

  const [regenTokenUserId,setRegenTokenUserId] = useState("");
  const [regenTokensError,setRegenTokensError] = useState(false);
  const [regenTokensSuccess,setRegenTokensSuccess] = useState(false);

  const [removeUserId, setRemoveUserId] = useState("");
  const [removeUserError, setRemoveUserError] = useState(false);
  const [removeUserSuccess, setRemoveUserSuccess] = useState(false);
  const [removeUserRoom, setRemoveUserRoom] = useState("Main");
  const [removeUserRoomMessages, setRemoveUserRoomMessages] = useState(false);


  const [removePasswordId, setRemovePasswordId] = useState("");
  const [removePasswordError, setRemovePasswordError] = useState(false);
  const [removePasswordSuccess, setRemovePasswordSuccess] = useState(false);





  useEffect(() => {
    fetch("http://localhost:5000/getAllRooms/")
      .then((response) => response.json())
      .then((data) => {
        setRooms(data);
      })
      .catch((e) => {
        console.error(e);
      });

    fetch("http://localhost:5000/getAllUsers/")
      .then((response) => response.json())
      .then((data) => {
        setDeleteUserId(data[0].id);
        setUsers(data);
        setAddUserId(data[0].id);
        setRegenTokenUserId(data[0].id);
        setRemoveUserId(data[0].id);
        setRemovePasswordId(data[0].id);

      })
      .catch((e) => {
        console.error("Error fetching users:", e);
      });
  }, []);

  function handleCreateRoomSubmit() {
    if (addRoomName === undefined || addRoomName === "") {
      setAddRoomError(true);
      setAddRoomSuccess(false);
    } else {
      const requestOptions = {
        method: "POST",
        headers: { "Content-Type": "application/json" },
      };
      fetch(
        `http://localhost:5000/createRoom/${encodeURIComponent(addRoomName)}`,
        requestOptions,
      )
        .then((res) => {
          if (res.ok === true) {
            setAddRoomSuccess(true);
            setAddRoomError(false);
            rooms.push(addRoomName);
          } else {
            setAddRoomSuccess(false);
            setAddRoomError(true);
          }
        })
        .catch((err) => console.error(err));
    }
  }

  useEffect(() => {
    fetch("http://localhost:5000/getAllUsers/")
      .then((response) => response.json())
      .then((data) => {
        setUsers(data);
      })
      .catch((err) => {
        console.error(err);
      });
  }, [createUserError, createUserSuccess]);

  function handleDeleteRoomSubmit() {
    const requestOptions = {
      method: "DELETE",
      headers: { "Content-Type": "application/json" },
    };
    fetch(
      `http://localhost:5000/deleteRoom/${encodeURIComponent(deleteRoom)}`,
      requestOptions,
    )
      .then((res) => {
        if (res.ok === true) {
          setRooms((rooms) => rooms.filter((room) => room !== deleteRoom));
          setDeleteRoomSuccess(true);
          setDeleteRoomError(false);
        } else {
          setDeleteRoomSuccess(false);
          setDeleteRoomError(true);
        }
      })
      .catch((err) => console.error(err));
  }

  function handleCreateUserSubmit() {
    const requestOptions = {
      method: "POST",
      headers: { "Content-Type": "application/json" },
    };
    if (createUser === undefined || createUser === "") {
      setCreateUserError(true);
      setCreateUserSuccess(false);
    } else {
      fetch(
        `http://localhost:5000/createUser/${encodeURIComponent(createUser)}`,
        requestOptions,
      )
        .then((res) => {
          if (res.ok === true) {
            setCreateUserError(false);
            setCreateUserSuccess(true);
          } else {
            setCreateUserError(true);
            setCreateUserSuccess(false);
          }
        })
        .catch((err) => console.error(err));
    }
  }

  function handleDeleteUserSubmit() {
    const requestOptions = {
      method: "DELETE",
      headers: { "Content-Type": "application/json" },
    };

    if (deleteUserId != localStorage.getItem("UserId")) {
      fetch(
        `http://localhost:5000/deleteUser/${encodeURIComponent(deleteUserId)}`,
        requestOptions,
      )
        .then((res) => {
          if (res.ok === true) {
            setUsers(users.filter((user) => user.id !== deleteUserId));
            setDeleteUserSuccess(true);
            setDeleteUserError(false);
          } else {
            setDeleteUserSuccess(false);
            setDeleteUserError(true);
          }
        })
        .catch((err) => console.error(err));
      if (deleteUserMessage == true) {
        fetch(
          `http://localhost:5000/deleteUserMessages/${encodeURIComponent(deleteUserId)}`,
          requestOptions,
        )
          .then((res) => {
            if (res.ok === true) {
              setDeleteUserError(false);
              setDeleteUserSuccess(true);
            } else {
              setDeleteUserError(true);
              setDeleteUserSuccess(false);
            }
          })
          .catch((err) => console.error(err));
      }
    } else {
      setDeleteUserError(true);
      setDeleteUserSuccess(false);
    }
  }

  function handleAddUserSubmit() {
    const requestOptions = {
      method: "POST",
      headers: { "Content-Type": "application/json" },
    };

    fetch(
      `http://localhost:5000/addUser/${encodeURIComponent(addUserId)}/${encodeURIComponent(addUserRoom)}`,
      requestOptions,
    )
      .then((res) => {
        if (res.ok === true) {
          setAddUserError(false);
          setAddUserSuccess(true);
        } else {
          setAddUserError(true);
          setAddUserSuccess(false);
        }
      })
      .catch((err) => console.error(err));
  }

    function handleRegenTokenSubmit() {
        const requestOptions = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
        };

        fetch(`http://localhost:5000/regenTokens/${encodeURIComponent(regenTokenUserId)}`,requestOptions).then(res=>{
            if(res.ok === true) {
                setRegenTokensError(false);
                setRegenTokensSuccess(true);
            }
            else
            {
                setRegenTokensError(true);
                setRegenTokensSuccess(false);
            }
        }).catch(err=>{console.error(err)});
    }

    function handleRemovePassword() {
        const requestOptions = {
            method: "DELETE",
            headers: { "Content-Type": "application/json" },
        };
        fetch(`http://localhost:5000/removePassword/${encodeURIComponent(removePasswordId)}`,requestOptions).then(res=>{
            if (res.ok === true) {
                setRemovePasswordSuccess(true);
                setRemovePasswordError(false);
            }
            else
            {
                setRemovePasswordSuccess(false);
                setRemovePasswordError(true);
            }
        })

    }

    function handleRemoveUser() {
      var requestOptions = {method: "DELETE", headers: { "Content-Type": "application/json" },};
      var fetchSuccess = false;
        fetch(`http://localhost:5000/removeUser/${encodeURIComponent(removeUserId)}/${encodeURIComponent(removeUserRoom
        )}`,requestOptions).then(res=>
        {
        }).catch(err=>{console.error(
            err)});
        if( removeUserRoomMessages===true) {


            fetch(`http://localhost:5000/deleteUserRoomMessages/${encodeURIComponent(removeUserId)}/${encodeURIComponent(removeUserRoom)}`,requestOptions).then(res=>
            {
                if(res.ok === true) {
                    setRemoveUserError(false);
                    setRemoveUserSuccess(true);
                }
                else
                {
                    setRemoveUserError(true);
                    setRemoveUserSuccess(false);
                }
            }).catch(err=>{console.error(err)});
        }
    }

    return (
        <div className={styles.parent}>
            <h1>Admin Panel</h1>
            <h2 className={styles.noteWarning}>
                NOTE: For changes to take effect server MUST be restart
            </h2>
            <h2>Create Room</h2>
            Create new room with name:{" "}
            <input
                type="text"
                onChange={(e) => setAddRoomName(e.target.value)}
            ></input>
            <br></br>
            {addRoomError && (
                <div className={styles.errorMessage}>
                    An error occured, the name is empty or already in use
                </div>
            )}
            {addRoomSuccess && (
                <div className={styles.successMessage}>Room created succesfully</div>
            )}
            <br></br>
            <button onClick={() => handleCreateRoomSubmit()}>Submit</button>
            <h2>Delete Room</h2>
            Delete room with name:
            <select onChange={(e) => setSelectedDeleteRoom(e.target.value)}>
                {rooms.map((room) => {
                    return <option value={room}>{room}</option>;
                })}
            </select>
            <br></br>
            {deleteRoomError && (
                <div className={styles.errorMessage}>
                    An error occured, main can't be deleted
                </div>
            )}
            {deleteRoomSuccess && (
                <div className={styles.successMessage}>Room deleted succesfully</div>
            )}
            <br></br>
            <button onClick={() => handleDeleteRoomSubmit()}>Submit</button>
            <h2>Create User</h2>
            Create new user with name:{" "}
            <input
                type="text"
                onChange={(e) => setCreateUser(e.target.value)}
            ></input>
            <br></br>
            {createUserError && (
                <div className={styles.errorMessage}>
                    An error occurred, user can't be created. Username must be non blank
                </div>
            )}
            {createUserSuccess && (
                <div className={styles.successMessage}>User created successfully</div>
            )}
            <br></br>
            <button onClick={() => handleCreateUserSubmit()}>Submit</button>
            <h2>Delete User</h2>
            Delete user with name:{" "}
            <select
                value={deleteUserId}
                onChange={(e) => setDeleteUserId(e.target.value)}
            >
                {users.map((user) => {
                    return <option value={user.id}>{user.username}</option>;
                })}
            </select>
            <br></br>
            Delete all user messages
            <input
                type="checkbox"
                value={deleteUserMessage}
                onChange={(e) => setDeleteUserMessage(!deleteUserMessage)}
            />
            <br></br>
            {deleteUserError && (
                <div className={styles.errorMessage}>
                    An error occurred, user can't be deleted
                </div>
            )}
            {deleteUserSuccess && (
                <div className={styles.successMessage}>User deleted successfully</div>
            )}
            <br></br>
            <button onClick={() => handleDeleteUserSubmit()}>Submit</button>
            <h2>Add user to existing room </h2>
            <select value={addUserId} onChange={(e) => setAddUserId(e.target.value)}>
                {users.map((user) => {
                    return <option value={user.id}>{user.username}</option>;
                })}
            </select>{" "}
            to room
            <select
                value={addUserRoom}
                onChange={(e) => setAddUserRoom(e.target.value)}
            >
                {rooms.map((room) => {
                    return <option value={room}>{room}</option>;
                })}
            </select>
            <br></br>
            {addUserError && (
                <div className={styles.errorMessage}>
                    An error occurred, user can't be added to room
                </div>
            )}
            {addUserSuccess && (
                <div className={styles.successMessage}>User added successfully</div>
            )}
            <br></br>
            <button onClick={() => handleAddUserSubmit()}>Submit</button>

            <h2>Remove user from existing room</h2>
            Remove <select value={removeUserId} onChange={(e) => setRemoveUserId(e.target.value)}>{users.map(user => {
            return <option value={user.id}>{user.username}</option>
        })}</select> from room
            <select value={removeUserRoom} onChange={(e) => setRemoveUserRoom(e.target.value)}>{rooms.map((room) => {
                return <option value={room}>{room}</option>
            })}</select>
            <br></br>
            Remove user messages from room
            <input type="checkbox" value={removeUserRoomMessages} onChange={()=>setRemoveUserRoomMessages(!removeUserRoomMessages)}/>
            <br></br>
            {removeUserError && (
                <div className={styles.errorMessage}>
                    User removal has encountered an error
                </div>
            )}
            {removeUserSuccess && (
                <div className={styles.successMessage}>User removed succesfully</div>
            )}
            <br></br>

            <button onClick={()=>handleRemoveUser()}>Submit</button>


            <h2>Regen tokens</h2>
            Regen tokens of user <select value={regenTokenUserId} onChange={(e) => setRegenTokenUserId(e.target.value)}>
            {
                users.map((user) => {
                    return <option value={user.id}>{user.username}</option>
                })
            }
        </select>
            <br></br>
            {regenTokensError && (
                <div className={styles.errorMessage}>
                    An error occured, tokens can't be regen;
                </div>
            )}
            {regenTokensSuccess && (
                <div className={styles.successMessage}>Tokens regen successfully</div>
            )}
            <br></br>

            <button onClick={() => handleRegenTokenSubmit()}>Submit</button>

            <h2>Remove password</h2>
            Remove password of user <select onChange={(e) => setRemovePasswordId(e.target.value)}>{users.map(user => {
            return <option value={user.id}>{user.username}</option>
        })}</select>
            (Note: Will also reset tokens)
            <br></br>
            {removePasswordError && (
                <div className={styles.errorMessage}>
                    Password remove function has encountered an error
                </div>
            )}
            {removePasswordSuccess && (
                <div className={styles.successMessage}>Password removed successfully</div>
            )}

            <br></br>
            <button onClick={() => handleRemovePassword()}>Submit</button>
        </div>
    );
}

export default AdminPage;