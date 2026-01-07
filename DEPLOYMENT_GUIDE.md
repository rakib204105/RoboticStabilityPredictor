# Azure Deployment Guide: Robotic Stability Predictor

This guide will walk you through deploying your ASP.NET Core application to Microsoft Azure using the Free Tier (F1 plan).

## Phase 1: Push Code to GitHub

Since we have already set up the repository locally, run the following command in your terminal to synchronize with GitHub:

```bash
git push -u origin main
```

> **Note**: If asked for a username/password:
> - **Username**: `rakib204105`
> - **Password**: Your GitHub Personal Access Token (or browser authentication if available).

---

## Phase 2: Create Azure Free Account

1. Go to [azure.microsoft.com/free](https://azure.microsoft.com/free/)
2. Click **Start free** and sign in with your Microsoft/GitHub account.
3. Follow the registration process (requires credit card for identity verification, but you won't be charged for free services).

---

## Phase 3: Create Web App

1. Log in to the [Azure Portal](https://portal.azure.com).
2. In the search bar, type **"App Services"** and select it.
3. Click **+ Create** > **Web App**.
4. Configure the basics:
   - **Subscription**: Select your subscription (e.g., "Azure subscription 1").
   - **Resource Group**: Click "Create new" and name it `RoboticStabilityRG`.
   - **Name**: Enter a unique name (e.g., `robotic-stability-predictor-<yourname>`).
   - **Publish**: Select **Code**.
   - **Runtime stack**: Select **.NET 8 (LTS)**.
   - **Operating System**: Select **Linux** (recommended) or Windows.
   - **Region**: Select a region close to you (e.g., `Central US` or `Southeast Asia`).
   - **Pricing Plan**:
     - Click **Change size** (or "Explore pricing plans").
     - Select **Free F1** (Shared infrastructure).
     - Click **Apply**.
5. Click **Review + create** and then **Create**.
6. Wait for deployment to complete (takes 1-2 minutes).

---

## Phase 4: Configure GitHub Deployment

1. Once creating is done, click **Go to resource**.
2. In the left menu, find **Deployment Center** (under "Deployment").
3. Under **Source**, select **GitHub**.
4. **Authorize** Azure to access your GitHub account if prompted.
5. Configure settings:
   - **Organization**: `rakib204105`
   - **Repository**: `RoboticStabilityPredictor`
   - **Branch**: `main`
6. Click **Save**.

Azure will now automatically:
- Detect the ASP.NET Core project.
- Build the application.
- Deploy it to your website.

You can view the progress in the **Logs** tab of Deployment Center.

---

## Phase 5: Verification

1. Wait for the status to show "Success" (Active).
2. Click **Browse** from the top menu or visit `https://<your-app-name>.azurewebsites.net`.
3. Verify the application loads.
4. Try registering a user (this tests the SQLite database).
5. Run a stability calculation and save to Excel.

### 💡 Excel Data Note
Your Excel file will be saved in the application's local storage. This works fine on Azure, but:
- It is specific to the running instance.
- It is NOT permanent (might be reset if you delete/recreate the app).
- For professional production use later, we can upgrade to Azure Blob Storage.

---

## 🎉 Congratulations!
Your application is now live on the cloud!
