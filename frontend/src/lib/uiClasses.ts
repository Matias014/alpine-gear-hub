// Small shared style tokens so buttons look the same everywhere they're used, rather than each
// page inlining its own slightly-different classes.
export const buttonPrimary =
  'inline-flex items-center justify-center rounded-lg bg-emerald-600 px-5 py-2.5 text-sm font-semibold ' +
  'text-white shadow-sm transition-colors hover:bg-emerald-500 disabled:opacity-50'

export const buttonSecondary =
  'inline-flex items-center justify-center rounded-lg border border-gray-300 bg-white px-5 py-2.5 text-sm ' +
  'font-semibold text-gray-700 shadow-sm transition-colors hover:bg-gray-50 disabled:opacity-50'

// For use on the dark hero gradient, where the plain secondary style would be invisible.
export const buttonOnDark =
  'inline-flex items-center justify-center rounded-lg border border-white/30 bg-white/10 px-5 py-2.5 text-sm ' +
  'font-semibold text-white backdrop-blur transition-colors hover:bg-white/20'
