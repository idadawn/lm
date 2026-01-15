@echo off
echo Upgrading pnpm to latest version...
call npm install -g pnpm@latest

echo Installing dependencies in web directory...
cd web
call pnpm install

echo Starting development server...
npm run dev
pause
