import { useState } from 'react'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { useUnreadMessagesCount } from '../hooks/useChat'
import { useChatNotifications } from '../hooks/useChatNotifications'

export function Layout() {
  const { user, isAuthenticated, logout } = useAuth()
  const unreadCount = useUnreadMessagesCount()
  useChatNotifications()
  const navigate = useNavigate()
  const location = useLocation()
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  // Logged-in nav has 6 items (Browse/My listings/Messages/Moderation/Log out/Sell gear) - too
  // many to fit a phone screen inline, so below md it collapses into this toggled menu instead.
  // Closing on every route change means a tapped link doesn't leave the menu stuck open behind it.
  // Adjusted during render (not an effect) so the closed menu shows up in the same paint as the
  // new page instead of flashing open for one frame first.
  const [lastPathname, setLastPathname] = useState(location.pathname)
  if (location.pathname !== lastPathname) {
    setLastPathname(location.pathname)
    setIsMenuOpen(false)
  }

  // Protected pages already redirect on logout via RequireAuth/RequireModerator, but public
  // pages (home, browse, listing detail) don't - without this, logging out while on e.g.
  // "My listings" just silently updates the nav and leaves you on a now-meaningless filtered view.
  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <div className="flex min-h-screen flex-col bg-gray-50">
      <header className="sticky top-0 z-10 border-b border-gray-200 bg-white/95 backdrop-blur">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
          <Link to="/" className="text-lg font-bold tracking-tight text-gray-900">
            Alpine<span className="text-emerald-600">Gear</span>Hub
          </Link>

          <nav className="hidden items-center gap-5 text-sm md:flex">
            <Link to="/listings" className="text-gray-600 transition-colors hover:text-gray-900">
              Browse
            </Link>

            {isAuthenticated && user ? (
              <>
                <Link
                  to={`/listings?sellerId=${user.id}`}
                  className="text-gray-600 transition-colors hover:text-gray-900"
                >
                  My listings
                </Link>
                <Link to="/messages" className="relative text-gray-600 transition-colors hover:text-gray-900">
                  Messages
                  {unreadCount > 0 && (
                    <span className="absolute -right-3 -top-2 rounded-full bg-emerald-600 px-1.5 py-0.5 text-[10px] font-medium text-white">
                      {unreadCount}
                    </span>
                  )}
                </Link>
                {(user.role === 'Moderator' || user.role === 'Admin') && (
                  <Link to="/moderation" className="text-gray-600 transition-colors hover:text-gray-900">
                    Moderation
                  </Link>
                )}
                <button
                  type="button"
                  onClick={handleLogout}
                  className="text-gray-600 transition-colors hover:text-gray-900"
                >
                  Log out
                </button>
                <Link
                  to="/listings/new"
                  className="rounded-lg bg-emerald-600 px-3 py-1.5 font-semibold text-white shadow-sm transition-colors hover:bg-emerald-500"
                >
                  Sell gear
                </Link>
              </>
            ) : (
              <>
                <Link to="/login" className="text-gray-600 transition-colors hover:text-gray-900">
                  Log in
                </Link>
                <Link
                  to="/register"
                  className="rounded-lg bg-emerald-600 px-3 py-1.5 font-semibold text-white shadow-sm transition-colors hover:bg-emerald-500"
                >
                  Register
                </Link>
              </>
            )}
          </nav>

          <button
            type="button"
            onClick={() => setIsMenuOpen((open) => !open)}
            className="rounded-lg p-2 text-gray-600 transition-colors hover:bg-gray-100 md:hidden"
            aria-label={isMenuOpen ? 'Close menu' : 'Open menu'}
            aria-expanded={isMenuOpen}
          >
            {isMenuOpen ? (
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M6 18 18 6M6 6l12 12" />
              </svg>
            ) : (
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            )}
          </button>
        </div>

        {isMenuOpen && (
          <nav className="flex flex-col gap-1 border-t border-gray-200 px-4 py-3 text-sm md:hidden">
            <Link to="/listings" className="rounded-lg px-3 py-2 text-gray-700 hover:bg-gray-50">
              Browse
            </Link>

            {isAuthenticated && user ? (
              <>
                <Link to={`/listings?sellerId=${user.id}`} className="rounded-lg px-3 py-2 text-gray-700 hover:bg-gray-50">
                  My listings
                </Link>
                <Link
                  to="/messages"
                  className="flex items-center justify-between rounded-lg px-3 py-2 text-gray-700 hover:bg-gray-50"
                >
                  Messages
                  {unreadCount > 0 && (
                    <span className="rounded-full bg-emerald-600 px-2 py-0.5 text-xs font-medium text-white">
                      {unreadCount}
                    </span>
                  )}
                </Link>
                {(user.role === 'Moderator' || user.role === 'Admin') && (
                  <Link to="/moderation" className="rounded-lg px-3 py-2 text-gray-700 hover:bg-gray-50">
                    Moderation
                  </Link>
                )}
                <Link
                  to="/listings/new"
                  className="mt-2 rounded-lg bg-emerald-600 px-3 py-2 text-center font-semibold text-white shadow-sm transition-colors hover:bg-emerald-500"
                >
                  Sell gear
                </Link>
                <button
                  type="button"
                  onClick={handleLogout}
                  className="rounded-lg px-3 py-2 text-left text-gray-700 hover:bg-gray-50"
                >
                  Log out
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="rounded-lg px-3 py-2 text-gray-700 hover:bg-gray-50">
                  Log in
                </Link>
                <Link
                  to="/register"
                  className="mt-2 rounded-lg bg-emerald-600 px-3 py-2 text-center font-semibold text-white shadow-sm transition-colors hover:bg-emerald-500"
                >
                  Register
                </Link>
              </>
            )}
          </nav>
        )}
      </header>

      {/* w-full is load-bearing here: main is a flex item of the flex-col wrapper above, and
          it uses mx-auto itself - without an explicit width, main hits the same auto-margins-
          override-stretch quirk (shrinks to content width instead of filling to max-w-5xl).
          Deliberately NOT flex/flex-col on main itself: that broke every child centering via its
          own mx-auto (ConversationPage, Create/EditListingPage) the same way, one level down. */}
      <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}
