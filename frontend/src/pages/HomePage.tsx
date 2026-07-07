import { Link } from 'react-router-dom'
import { ListingCard } from '../components/ListingCard'
import { useAuth } from '../contexts/AuthContext'
import { useListings } from '../hooks/useListings'
import { buttonOnDark, buttonPrimary } from '../lib/uiClasses'

export default function HomePage() {
  const { user, isAuthenticated, isLoading } = useAuth()
  const { data: recentListings } = useListings({ page: 1, pageSize: 4 })

  if (isLoading) return null

  return (
    <div className="space-y-12">
      {isAuthenticated && user ? <AuthenticatedHero user={user} /> : <GuestHero />}

      {!isAuthenticated && <ValueProps />}

      {recentListings && recentListings.items.length > 0 && (
        <section>
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-gray-900">Fresh on the marketplace</h2>
            <Link to="/listings" className="text-sm font-medium text-emerald-700 hover:underline">
              Browse all &rarr;
            </Link>
          </div>
          <div className="mt-4 grid grid-cols-2 items-start gap-4 sm:grid-cols-3 md:grid-cols-4">
            {recentListings.items.map((listing) => (
              <ListingCard key={listing.id} listing={listing} />
            ))}
          </div>
        </section>
      )}
    </div>
  )
}

function GuestHero() {
  return (
    <section className="overflow-hidden rounded-2xl bg-linear-to-br from-slate-900 via-emerald-900 to-emerald-700 px-6 py-16 text-center shadow-lg sm:px-12">
      <p className="text-sm font-semibold uppercase tracking-wider text-emerald-300">
        Climbing &amp; mountaineering gear, peer to peer
      </p>
      <h1 className="mx-auto mt-3 max-w-2xl text-4xl font-bold tracking-tight text-white sm:text-5xl">
        Gear you can trust, from climbers who get it.
      </h1>
      <p className="mx-auto mt-4 max-w-xl text-base text-emerald-100">
        Buy and sell safety-certified ropes, harnesses, and hardware directly with other climbers. No
        middlemen, no guesswork — just gear that&apos;s been checked and cared for.
      </p>
      <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
        <Link to="/listings" className={buttonPrimary}>
          Browse gear
        </Link>
        <Link to="/register" className={buttonOnDark}>
          Start selling
        </Link>
      </div>
    </section>
  )
}

interface AuthenticatedHeroProps {
  user: { id: string; fullName: string; role: string }
}

function AuthenticatedHero({ user }: AuthenticatedHeroProps) {
  return (
    <section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm sm:p-8">
      <p className="text-sm text-gray-500">Welcome back</p>
      <h1 className="mt-1 text-2xl font-bold text-gray-900">
        {user.fullName} <span className="text-base font-normal text-gray-400">· {user.role}</span>
      </h1>

      <div className="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-4">
        <QuickLink to="/listings" label="Browse gear" />
        <QuickLink to="/listings/new" label="Sell gear" />
        <QuickLink to="/messages" label="Messages" />
        <QuickLink to={`/listings?sellerId=${user.id}`} label="My listings" />
      </div>
    </section>
  )
}

function QuickLink({ to, label }: { to: string; label: string }) {
  return (
    <Link
      to={to}
      className="rounded-lg border border-gray-200 bg-gray-50 px-4 py-3 text-center text-sm font-medium text-gray-700 transition-colors hover:border-emerald-300 hover:bg-emerald-50 hover:text-emerald-800"
    >
      {label}
    </Link>
  )
}

const FEATURES = [
  {
    title: 'Safety-first listings',
    description: 'Every listing includes condition and certification details, so you know exactly what you\'re buying.',
  },
  {
    title: 'Chat with sellers directly',
    description: 'Ask questions and arrange a sale in real time — no email back-and-forth.',
  },
  {
    title: 'A moderated community',
    description: 'Reports are reviewed by real moderators, keeping listings honest.',
  },
]

function ValueProps() {
  return (
    <section className="grid gap-6 sm:grid-cols-3">
      {FEATURES.map((feature) => (
        <div key={feature.title} className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
          <h3 className="font-semibold text-gray-900">{feature.title}</h3>
          <p className="mt-1 text-sm text-gray-600">{feature.description}</p>
        </div>
      ))}
    </section>
  )
}
