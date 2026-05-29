import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Enable standalone output for Docker
  output: 'standalone',
  // Disable typechecking during build to save RAM on low-resource machines.
  typescript: { ignoreBuildErrors: true },
};

export default nextConfig;
