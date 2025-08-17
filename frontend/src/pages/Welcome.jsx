import { Link } from 'react-router-dom';

export default function Welcome() {
  return (
    <div className="py-5 text-center">
      <h1 className="display-5 fw-bold">Welcome to Graphigrade</h1>
      <p className="lead text-muted mt-3">A simple system for grading computer graphics student solutions.</p>
      <div className="d-flex gap-3 justify-content-center mt-4">
        <Link className="btn btn-primary btn-lg" to="/login">Login</Link>
        <Link className="btn btn-outline-primary btn-lg" to="/register">Register</Link>
      </div>
    </div>
  );
}
