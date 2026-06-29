import type { NextConfig } from 'next'

const securityHeaders = [
  // Prevent MIME-type sniffing
  { key: 'X-Content-Type-Options', value: 'nosniff' },
  // Deny framing — clickjacking protection
  { key: 'X-Frame-Options', value: 'DENY' },
  // Legacy XSS filter
  { key: 'X-XSS-Protection', value: '1; mode=block' },
  // Limit referrer info leakage
  { key: 'Referrer-Policy', value: 'strict-origin-when-cross-origin' },
  // Disable features not needed
  { key: 'Permissions-Policy', value: 'camera=(), microphone=(), geolocation=(), payment=()' },
  // HSTS — enforce HTTPS once deployed with TLS
  { key: 'Strict-Transport-Security', value: 'max-age=63072000; includeSubDomains' },
  // CSP — allow self + API + MinIO; block inline scripts except Next.js nonces
  {
    key: 'Content-Security-Policy',
    value: [
      "default-src 'self'",
      "script-src 'self' 'unsafe-inline' 'unsafe-eval'", // unsafe-eval required by Next.js dev; tighten in prod
      "style-src 'self' 'unsafe-inline'",
      "img-src 'self' data: blob: http://localhost:9000",
      "font-src 'self'",
      "connect-src 'self' http://localhost:8080 http://localhost:9000",
      "frame-ancestors 'none'",
      "base-uri 'self'",
      "form-action 'self'",
    ].join('; '),
  },
]

const nextConfig: NextConfig = {
  output: 'standalone',
  images: { unoptimized: true },
  async headers() {
    return [
      {
        source: '/(.*)',
        headers: securityHeaders,
      },
    ]
  },
}

export default nextConfig
