import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import basicSsl from "@vitejs/plugin-basic-ssl";

export default defineConfig({
  plugins: [react(), basicSsl()],
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
