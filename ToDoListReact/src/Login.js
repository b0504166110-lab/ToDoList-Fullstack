import React, { useState } from 'react';
import service from './service.js';

function Login({ onLogin }) {
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [isRegister, setIsRegister] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    try {
      if (isRegister) await service.register(userName, password);
      await service.login(userName, password);
      onLogin();
    } catch (err) {
      if (err.response?.status === 409) setError("שם המשתמש תפוס");
      else if (err.response?.status === 401) setError("שם משתמש או סיסמה שגויים");
      else setError("שגיאה, נסי שוב");
    }
  }

  return (
    <section className="todoapp">
      <header className="header">
        <h1>{isRegister ? "הרשמה" : "התחברות"}</h1>
        <form onSubmit={handleSubmit}>
          <input className="new-todo" placeholder="שם משתמש" value={userName} onChange={e => setUserName(e.target.value)} />
          <input className="new-todo" type="password" placeholder="סיסמה" value={password} onChange={e => setPassword(e.target.value)} />
          <button type="submit">{isRegister ? "הרשמה" : "התחברות"}</button>
        </form>
        {error && <p>{error}</p>}
        <button onClick={() => setIsRegister(!isRegister)}>
          {isRegister ? "יש לי חשבון, להתחברות" : "אין לי חשבון, להרשמה"}
        </button>
      </header>
    </section>
  );
}

export default Login;