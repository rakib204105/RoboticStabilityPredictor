# Render Deployment Guide: Robotic Stability Predictor

This guide will walk you through deploying your ASP.NET Core application to **Render.com** using the Free Tier (No credit card required).

## Phase 1: Push Changes (Done!)

I have already created the `Dockerfile` and pushed it to your GitHub repository.

---

## Phase 2: Create Render Account

1. Go to [dashboard.render.com/register](https://dashboard.render.com/register).
2. Click **Sign up with GitHub**.
3. Authorize Render to access your GitHub account.

---

## Phase 3: Create Web Service

1. On the Render Dashboard, click **New +** and select **Web Service**.
2. Select **"Build and deploy from a Git repository"**.
3. Find `RoboticStabilityPredictor` in the list and click **Connect**.
   - *Note: If you don't see it, click "Configure account" on the right to grant permissions to the repo.*

---

## Phase 4: Configure Service

Render will detect the Dockerfile automatically. Configure the following:

- **Name**: `robotic-stability-app` (or any unique name)
- **Region**: Select singular (e.g., Singapore or Oregon)
- **Branch**: `main`
- **Root Directory**: (Leave blank)
- **Runtime**: **Docker** (Important! Do NOT select .NET Core)
  - It should auto-select "Docker" because I added a Dockerfile.
- **Instance Type**: **Free**

Click **Create Web Service**.

---

## Phase 5: Wait & Verify

1. You will see the logs building your Docker image.
2. It usually takes 3-5 minutes for the first build.
3. Once completed, you will see "Your service is live" and a URL (e.g., `https://robotic-stability-app.onrender.com`).
4. Click the URL to test your app!

---

## ⚠️ Important Note About Free Tier

**Ephemeral Storage**:
Render's free tier files are temporary.
- If the app restarts (which happens automatically on free tier), **registered users and Excel data will be reset**.
- This is perfectly fine for a university demo or portfolio project.
- If you need permanent data later, you can connect a managed database (PostgreSQL), but sticking to the free tier limitations is easiest for now.

**Spin Down**:
- The free plan "spins down" (goes to sleep) after 15 minutes of inactivity.
- The next time you visit, it might take **50-60 seconds to load**. This is normal.
