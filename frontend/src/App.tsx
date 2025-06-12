import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import Co2Readings from './components/Co2Readings';
import Rooms from './components/Rooms';
import './App.css';

function App() {
  return (
    <Router>
      <header className="app-header">
        <nav className="navbar">
          <div className="navbar-title">CO2 Monitor</div>
          <div className="navbar-links">
            <Link to="/" className="nav-link">Readings</Link>
            <Link to="/rooms" className="nav-link">Rooms</Link>
          </div>
        </nav>
      </header>
      <main className="main-content">
        <Routes>
          <Route path="/" element={<Co2Readings />} />
          <Route path="/rooms" element={<Rooms />} />
        </Routes>
      </main>
    </Router>
  );
}

export default App;
