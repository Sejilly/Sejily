import "server-only";

export const API_BASE_URL = process.env.API_BASE_URL ?? "";

export const TOKEN_COOKIE = "sejily_token";

export const backendHeaders = {
  "Content-Type": "application/json",
  "ngrok-skip-browser-warning": "true",
};