# seq-logging-aspnetcore
A very simple example showing why we want structured logs

I ran across some code that was using string interpolation for logging messages. Generally, when logging in modern .NET, we should prefer structured log messages. This involves sending in parameters to the logger interface rather than simply sending an interpolated string.

I hope to show why you should prefer the latter with this simple repository.

## Running the application

This repository includes a `docker-compose` file for [Seq](https://datalust.co/seq), a wonderful centralized application logging platform that may fit your needs well. But, importantly for our needs, it helps with illustrating structured logging and why it's so powerful.

First, start up the container:

```powershell
docker-compose up
```

In another console, you can run the application code via the command line as well

```powershell
dotnet run
```

The code will log to the console and to Seq; using Serilog as the logging library. There are two endpoints. One uses structured logging and the other uses string interpolation.

After starting the application, note the URL that it's listening on and hit the root URL in your favorite browser or http client. 

You will see some output similar to this:

```powershell
[15:20:05 INF] Starting up
[15:20:05 INF] Now listening on: https://localhost:7273
[15:20:05 INF] Now listening on: http://localhost:5273
[15:20:05 INF] Application started. Press Ctrl+C to shut down.
[15:20:05 INF] Hosting environment: Development
[15:20:05 INF] Content root path: C:\code\seq-logging-aspnetcore\
[15:20:22 INF] Received request d3572b5d-5229-40fb-8365-eb858bd7b4c0 for a085c04b-4155-4d48-a91e-91208e60d4e5
[15:20:22 INF] Doing work for a085c04b-4155-4d48-a91e-91208e60d4e5
[15:20:22 INF] HTTP GET / responded 200 in 14.5300 ms
[15:20:22 INF] HTTP GET /favicon.ico responded 404 in 0.1886 ms
```

What's important, but you can't see in text above, is that the values for the correlation id and customer id are passed in as structured log values. As far as any stdio consumer is concerned, it's just text.

Here's a screenshot with some better highlighting

![Structured logging to console](https://user-images.githubusercontent.com/213495/156053302-3bf1bb49-766d-4d65-97cf-90617ba92860.png)

To trigger the interpolated logging, go to the same application URL but this time add `/inter` to the URL

I won't re-paste the interpolated text but you can see some subtle difference in the console output in this screenshot.

![interpolated logging to console](https://user-images.githubusercontent.com/213495/156053687-136a3e5b-d211-4b8d-97b2-b6a19e52930d.png)

As you can above, the strings are still there. They're missing their color-coding (to be fair, the first example would too if I used a non-colored logger). But, that only hints at the actual downstream impact.

## Why structured logging

So, now that you've hit both endpoints, let's go over to Seq and see how the examples translate over there.

Unless you've edited the `docker-compose` file, you can reach Seq's UI at http://localhost:8041. After having hit both the structured and interpolated endpoints, the screen should show something like the following (I've expanded the two entries that I want to talk about)

![Screenshot of log statements in Seq](https://user-images.githubusercontent.com/213495/156054817-99bce753-2d01-4dd8-8ac9-7111196b9e85.png)

Because of the settings I've enabled in Serilog, some common structured data is already logged in both. But, also note the main difference between the interpolated and structured logged statements. They both have the same log message, but the structured entry _also_ has the parameters stored as separate metadata. In a tool like Seq, this lets me quickly develop new search facets on data. 

Now to find all log entries with a metadata key/value of `CustomerId = '3b0c4130-6bad-43e7-a1ae-21e699f32b69'`, I can add that to my search. I can then also pull that off to the side to build up multiple filters. While this particular scenario is specific to Seq, you'll find similar capabilities in other tools.

By contrast, with the interpolated approach, I would need to search just for text matches in the log message. You can still build up facets and filters based on just text, but you lose the context.

For example, consider an application where you allow messaging between users. If you use structured logging and add the sender and recipient ids as structured data like so:

```csharp
Logger.LogInformation("Sending message from {FromUserId} to {ToUserId}", fromId, toId);
```

You can now quickly parse your logs to find all of the log entries related to messages where `user-3312` is the sender. With string interpolation, you'd need to hope you have enough text to disambiguate your results.

The value doesn't quite come through in a small and contrived example like this, but it's very important in distributed systems to be able to slice across many log entries and trace a user-initiated action through all of the components.

## Structure logging things I know

- It's powerful, especially when combined with a centralized logging solution that aggregates distributed components
- It's most useful when you take care to have common names for things. Otherwise you're back to just text searching
- Like the log message contents, care needs to be taken with regards to what gets logged (GDPR, PII, etc)
- Seq + Serilog is awesome for .net developer
