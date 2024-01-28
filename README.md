# Letterboxd to Telegram Bot

## Overview

This Telegram bot is designed to automatically post Letterboxd entries of specific members directly onto a Telegram channel. It simplifies the process of sharing movie reviews, ratings, and lists with the community on the designated Telegram channel named `cinephilesclubbbb`.

## Features

- **Automatic Posting:** The bot fetches Letterboxd entries from specified members and posts them on the Telegram channel.
  
- **Customization:** Users can configure the bot to target specific Letterboxd members and customize the posting frequency.

## Requirements

- .NET 8
- Nuget packages
  - HtmlAgilityPack : For parsing letterboxd RSS feed entries
  - Microsoft.Data.Sqlite : For checkpointing data to ensure duplicate entries are not posted
  - Microsoft.Data.Sqlite.Core : Same as above
  - SQLite : Same as above
  - Telegram.Bot : For posting to channel
- Env Variables
  - CHAT_ID : Chat ID of the channel where the message should be posted to
  - CINEPHILE_TOKEN : Bot token of the telegram bot that will be posting the message
  - RSS_URLS : Comma separated values of the RSS feeds from letterboxd profile pages
  - USERNAME_CREATOR_MAPPING : Key:value pairs of custom usernames to sign messages with. (key is the letterboxd username, value is custom username)

## Setup

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/your-username/letterboxd-telegram-bot.git
   cd letterboxd-telegram-bot
2. **Set Environment Variables**
3. **Run the exe**
