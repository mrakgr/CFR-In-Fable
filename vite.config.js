import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import {resolve} from 'node:path'

const config = {
    PORT: 8080,
    PROXY_PORT: 5000,
    ROOT: "src/client",
    DEPLOY: "deploy/public"
}

// https://vitejs.dev/config/
export default ({mode}) => {
    const target = `http://localhost:${config.PROXY_PORT}`

    return defineConfig({
        root: config.ROOT,
        build: {
            outDir: resolve(process.cwd(), config.DEPLOY)
        },
        server: {
            port: config.PORT,
            // proxy: {
            //     "/api": {target, changeOrigin: true},
            //     "/socket": {target, ws: true}
            //     }
            },
        plugins: [react()],
        define: {
            // remotedev will throw an exception without this.
            global: {}
            }
        })
}
