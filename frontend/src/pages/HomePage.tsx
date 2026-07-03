import { Link } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

export default function HomePage() {
  const { user, isAuthenticated, isLoading, logout } = useAuth()

  if (isLoading) return null

  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-8 text-center">
      <h1 className="text-2xl font-bold">AlpineGearHub</h1>

      {isAuthenticated && user ? (
        <>
          <p className="text-gray-600">
            Welcome back, <span className="font-medium">{user.fullName}</span> ({user.role})
          </p>
          <button type="button" onClick={logout} className="text-sm text-emerald-700 hover:underline">
            Log out
          </button>
        </>
      ) : (
        <div className="flex gap-4">
          <Link to="/login" className="text-emerald-700 hover:underline">
            Log in
          </Link>
          <Link to="/register" className="text-emerald-700 hover:underline">
            Register
          </Link>
        </div>
      )}
    </div>
  )
}
