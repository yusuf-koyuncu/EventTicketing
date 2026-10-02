# Angular client

The web client of the Event & Ticket Sales Platform, built with Angular 21.

See the [main README](../README.md) for an overview, screenshots and how to run the whole project. To start only the client (the API must be running on `http://localhost:5080`):

```bash
npm install
npm start
```

The app is served at `http://localhost:4200`, and requests to `/api` are proxied to the API (see `proxy.conf.json`).
