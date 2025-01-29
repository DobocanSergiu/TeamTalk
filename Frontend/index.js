import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import App from "./App";
import MainPage from "../src/Components/MainPage/MainPage";
import LoginPage from "../src/Components/LoginPage/LoginPage";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";

import reportWebVitals from "./reportWebVitals";
import AdminPage from "./Components/AdminPage/AdminPage";
import ForgotPasswordPage from "./Components/ForgotPasswordPage/ForgotPasswordPage";
import FirstLoginPage from "./Components/FirstLoginPage/FirstLoginPage";

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(
  <React.StrictMode>
    <Router>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/main" element={<MainPage />} />
          <Route path="/admin" element={<AdminPage/>}/>
          <Route path="/forgotpassword" element={<ForgotPasswordPage />} />
          <Route path="/firstlogin" element={<FirstLoginPage/>} />
      </Routes>
    </Router>
  </React.StrictMode>,
);

reportWebVitals();
