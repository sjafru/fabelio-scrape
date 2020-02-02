# Fabelio Scraper

[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=sjafru_fabelio-scrape)](https://sonarcloud.io/dashboard?id=sjafru_fabelio-scrape)

## What is Fabelio Scraper

FABELIO SCRAPER adalah website informasi sederhana yang contentnya berasal dari site Fabelio.com.
Yang terdiri dari aplikasi:

### Frontend: FabelioScrape.Web

Stack: 

- DotNet Core 3
- React JS
- Redux
- ReactStrap

### Backend: FabelioScrape.WebApi

Stack: 

- DotNet Core 3 dengan Clean Architecture.
- RestFull
- WebDriver Chrome Selenium

## How It Works

- User memasukkan Product Url dari Fabelio.com, dihalaman Frontend. Kemudian Frontend akan mengirimkan permintaan Rest ke Backend.
- Akan ada response error jika url bukan product dari fabelio.com. Dan apabila valid, backend akan menjalankan chrome instance dengan bantuan selenim web driver untuk melakukan sync content & ajax content. Image akan di download dalam static folder "wwwroot/images"
- Sementara itu Frontend akan berpindah ke halaman product detail, dengan status pending jikalau backend masih melakukan sinkron dengan site fabelio.com.
- Agar dapat melihat product-product yang telah di submit, silahkan mengklik menu "Products" pada menu utama.
- Background Service pada Backend akan menjalankan job sync content perjam atau berdasarkan value konfigurasi.


## Requirements

- Google Chrome
- DotNet Core SDK 3.1

## Development

Buka folder dengan VS Code.

Open terminal folder FabelioScrape.WebApi
Bash Shell

```
dotnet build && dotnet run
```

Open terminal folder FabelioScrape.Web
Bash Shell
```
dotnet build && dotnet run
```

## Deployment

### FabelioScrape.WebApi

Harus di deploy dalam container atau virtual machine yang telah terinstall
- Google Chrome
- DotNet Core Runtime 3.1 

### FabelioScrape.Web

Dalam linux container atau virtual machine yang telah terinstall DotNet Core Runtime 3.1

## help
