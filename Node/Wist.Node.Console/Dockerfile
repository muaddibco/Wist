FROM microsoft/dotnet-framework:4.7.1-windowsservercore-ltsc2016
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
EXPOSE 5051/tcp
EXPOSE 5052/udp
ENTRYPOINT ["C:\\app\\Wist.Node.Console.exe"]
