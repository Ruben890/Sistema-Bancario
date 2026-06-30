import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import mkcert from "vite-plugin-mkcert";

export default defineConfig({
  plugins: [react(), mkcert()],
  server: {
    https: {},
    port: 5173,
    strictPort: true
  },
  preview: {
    https: {},
    port: 4173
  }
});