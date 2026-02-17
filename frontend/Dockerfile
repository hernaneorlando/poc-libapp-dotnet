FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./frontend/ ./
RUN dotnet restore "Web/Web.csproj"
RUN dotnet publish "Web/Web.csproj" -c release -o /app --no-restore

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/wwwroot .
COPY <<EOF /etc/nginx/conf.d/default.conf
server {
    listen 8080;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;
    
    # Permitir arquivos de configuração necessários
    location = /appsettings.json {
        try_files \$uri =404;
    }
    
    location = /appsettings.Production.json {
        try_files \$uri =404;
    }
    
    # Bloquear arquivos de configuração específicos por ambiente (SEGURANÇA)
    location ~ /appsettings\.(Development|Local)\.json\$ {
        deny all;
        return 404;
    }
    
    # Bloquear acesso a outros arquivos sensíveis
    location ~ /\.(env|git|svn) {
        deny all;
        return 404;
    }
    
    location / {
        try_files \$uri \$uri/ /index.html;
    }
    
    # Enable gzip for better performance
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript application/wasm;
}
EOF
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
