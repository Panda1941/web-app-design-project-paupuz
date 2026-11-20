# LoL Builds

Saityno taikomųjų programų projektavimo modulio projektas – „League of Legends“ čempionų build’ų kūrimo ir aptarimo platforma.

Šiame dokumente aprašyta projekto užduotis, realizuota sistema, jos API ir paleidimas.

## 1. Sprendžiamo uždavinio aprašymas

### 1.1. Sistemos paskirtis

„League of Legends“ žaidėjai tam pačiam čempionui gali naudoti skirtingus daiktus (items), burtus (summoner spells) ir runas (runes), priklausomai nuo žaidimo pozicijos bei pasirinktos strategijos. Norint pasidalinti tokiu pasirinkimu, reikia pateikti ne tik patį build’ą, bet ir paaiškinti, kada ir kaip jį naudoti.

Projekto tikslas – sukurti internetinę platformą, kurioje naudotojai galėtų kurti čempionams skirtus build’us, dalintis jais ir aptarti juos komentaruose. Build’ą sudarys pavadinimas, aprašymas, žaidimo pozicija, pasirinkti daiktai, summoner spell'ai ir runos.

Čempionai iš anksto įkelti į sistemą, o jų informaciją valdo administratorius. Build’us ir komentarus kuria registruoti naudotojai. Neprisijungę lankytojai gali peržiūrėti viešai pateiktą turinį.

### 1.2. Taikomosios srities objektai

Sistemoje yra trys pagrindiniai objektai:

| Objektas | Paskirtis ir pagrindinė informacija |
| --- | --- |
| **Čempionas (Champion)** | Žaidimo čempionas, kuriam kuriami build’ai. Saugomas jo pavadinimas, titulas, aprašymas ir paveikslėlio nuoroda. |
| **Build’as (Build)** | Konkrečiam čempionui skirtas naudotojo sukurtas build’as. Saugomas pavadinimas, aprašymas, žaidimo pozicija, pasirinkti daiktai, burtai, runos, autorius ir sukūrimo bei atnaujinimo laikas. |
| **Komentaras (Comment)** | Naudotojo atsiliepimas arba klausimas po konkrečiu build’u. Saugomas tekstas, autorius ir sukūrimo bei atnaujinimo laikas. |

Objektai sudaro hierarchiją **Champion → Build → Comment**:

- Vienam čempionui gali priklausyti daug build’ų. Kiekvienas build’as priklauso vienam čempionui.
- Vienam build’ui gali priklausyti daug komentarų. Kiekvienas komentaras priklauso vienam build’ui.

Naudotojų paskyros saugomos atskirai ir naudojamos autentifikacijai, autorizacijai bei turinio autorystei nustatyti. Jos nėra vienas iš trijų pagrindinių taikomosios srities objektų.

Daiktai, burtai ir runos pasirenkami iš iš anksto parengto katalogo. Pasirinkta kombinacija saugoma kaip build’o dalis; atskiros jų CRUD valdymo funkcijos į pradinę projekto apimtį neįtraukiamos.

### 1.3. Funkciniai reikalavimai

#### Svečias

Neprisijungęs naudotojas gali:

1. Peržiūrėti čempionų sąrašą ir konkretaus čempiono informaciją.
2. Peržiūrėti pasirinktam čempionui sukurtus build’us.
3. Peržiūrėti konkretaus build’o aprašymą, daiktus, burtus ir runas.
4. Peržiūrėti build’o komentarus.
5. Užsiregistruoti ir prisijungti prie sistemos.

#### Registruotas naudotojas

Prisijungęs naudotojas gali naudotis svečiui prieinamomis turinio peržiūros funkcijomis ir papildomai:

1. Sukurti pasirinktam čempionui skirtą build’ą: nurodyti jo pavadinimą, aprašymą ir žaidimo poziciją, pasirinkti daiktus, burtus bei runas.
2. Redaguoti ir pašalinti savo sukurtus build’us.
3. Rašyti komentarus po build’ais.
4. Redaguoti ir pašalinti savo komentarus.
5. Atsijungti nuo sistemos.

Registruotas naudotojas negali redaguoti ar pašalinti kitų naudotojų build’ų ir komentarų. Šios teisės tikrinamos serverio pusėje pagal prisijungusio naudotojo identifikatorių.

#### Administratorius

Administratorius gali naudotis registruotam naudotojui prieinamomis funkcijomis ir papildomai:

1. Pridėti naują čempioną.
2. Redaguoti čempiono informaciją.
3. Pašalinti čempioną.
4. Pašalinti netinkamus build’us ir komentarus, nepriklausomai nuo jų autoriaus.

## 2. REST API operacijos

Kiekvienam pagrindiniam objektui realizuotos penkios operacijos: sukūrimas, vieno objekto gavimas, atnaujinimas, pašalinimas ir sąrašo gavimas. Iš viso yra **15 taikomosios srities API metodų**. Papildomai realizuoti suvestinės, registracijos, prisijungimo, atsijungimo ir žetonų atnaujinimo metodai.

| Objektas | Metodas | Kelias | Paskirtis |
| --- | --- | --- | --- |
| Champion | GET | `/api/v1/champions` | Gauti čempionų sąrašą. |
| Champion | POST | `/api/v1/champions` | Sukurti čempioną. |
| Champion | GET | `/api/v1/champions/{championId}` | Gauti konkretų čempioną. |
| Champion | PUT | `/api/v1/champions/{championId}` | Atnaujinti čempioną. |
| Champion | DELETE | `/api/v1/champions/{championId}` | Pašalinti čempioną. |
| Build | GET | `/api/v1/champions/{championId}/builds` | Gauti konkretaus čempiono build’ų sąrašą. |
| Build | POST | `/api/v1/champions/{championId}/builds` | Sukurti build’ą konkrečiam čempionui. |
| Build | GET | `/api/v1/champions/{championId}/builds/{buildId}` | Gauti konkretų čempiono build’ą. |
| Build | PUT | `/api/v1/champions/{championId}/builds/{buildId}` | Atnaujinti build’ą. |
| Build | DELETE | `/api/v1/champions/{championId}/builds/{buildId}` | Pašalinti build’ą. |
| Comment | GET | `/api/v1/champions/{championId}/builds/{buildId}/comments` | Gauti konkretaus build’o komentarų sąrašą. |
| Comment | POST | `/api/v1/champions/{championId}/builds/{buildId}/comments` | Sukurti komentarą po konkrečiu build’u. |
| Comment | GET | `/api/v1/champions/{championId}/builds/{buildId}/comments/{commentId}` | Gauti konkretų build’o komentarą. |
| Comment | PUT | `/api/v1/champions/{championId}/builds/{buildId}/comments/{commentId}` | Atnaujinti komentarą. |
| Comment | DELETE | `/api/v1/champions/{championId}/builds/{buildId}/comments/{commentId}` | Pašalinti komentarą. |

Komentarų keliai apima visus tris hierarchijos objektus. Serveris tikrins, ar build’as priklauso nurodytam čempionui, o komentaras – nurodytam build’ui.

Papildomai:

- **Puslapiavimas** – visi sąrašų metodai priima `page` ir `pageSize` (1–50) parametrus ir grąžina `items`, `page`, `pageSize`, `totalCount`, `totalPages`.
- **Filtravimas** – čempionai pagal pavadinimą (`?name=ah`), build’ai pagal poziciją (`?position=Mid`), komentarai pagal tekstą (`?search=zhonya`).
- **Hypermedia** – kiekvienas objektas ir sąrašas turi `links` masyvą su susijusiais keliais ir leidžiamais veiksmais (`self`, `update`, `delete`, `builds`, `comments`, `next`, `previous` ir kt.).
- **Suvestinė iš kelių esybių** – `GET /api/v1/champions/{championId}/summary` grąžina čempioną, jo build’ų ir komentarų skaičių, build’ų pasiskirstymą pagal pozicijas ir naujausius build’us.

Atsako kodai: `200` – sėkminga užklausa, `201` – sukurta (su `Location` antrašte), `204` – ištrinta, `400` – neperskaitomas JSON arba netinkami reikšmių tipai, `404` – objektas nerastas arba nepriklauso nurodytam tėviniam objektui, `409` – toks čempionas jau yra, `422` – JSON teisingas, bet reikšmės neatitinka taisyklių.

API grąžina duomenis JSON formatu ir užklausos rezultatą atitinkančius HTTP būsenos kodus. Visiems prieigos taškams parengtas Postman užklausų rinkinys su testais (`postman/`). Duomenų bazė užpildoma prasmingais pradiniais duomenimis: 8 naudotojai, 10 čempionų, 22 build’ai ir 45 komentarai.

Pilna OpenAPI specifikacija su pavyzdžiais ir galimais atsakais pateikta faile [`docs/openapi.json`](docs/openapi.json), o paleidus API ją galima naršyti per Swagger UI (`/swagger`).

## 3. Pasirinktos technologijos

Frontend ir backend technologijos pasirinktos atsižvelgiant į patirtį su kitais projektais. Duomenų bazei pasirinkta SQLite, prieigai prie jos – Entity Framework Core.

| Sistemos dalis | Technologija | Pasirinkimo pagrindimas |
| --- | --- | --- |
| Kliento dalis | **React ir TypeScript** | React komponentai leidžia kurti ir pakartotinai naudoti čempionų, build’ų bei komentarų sąsajos elementus. TypeScript padeda aprašyti API duomenų struktūras ir aptikti tipų klaidas kuriant programą. |
| Serverio dalis | **C# ir ASP.NET Core** | Naudojami REST API, duomenų tikrinimui, autentifikacijai ir autorizacijai realizuoti. Šios technologijos jau naudojamos kituose projektuose. |
| Duomenų bazė | **SQLite** | Reliacinė duomenų bazė, laikoma viename faile ir nereikalaujanti atskiro serverio. Užtenka šios sistemos apkrovai, palaiko išorinius raktus ir kaskadinį trynimą. Esant daug lygiagrečių rašymų, tinkamesnė būtų PostgreSQL. |
| Prieiga prie duomenų bazės | **Entity Framework Core** | Duomenų modelis aprašomas C# klasėmis. Paleidus programą, duomenų bazė sukuriama ir užpildoma pradiniais duomenimis automatiškai. |

### 3.1. Autentifikacija ir autorizacija

Naudojami JWT prieigos žetonai ir atnaujinimo (refresh) žetonai.

| Metodas | Kelias | Paskirtis |
| --- | --- | --- |
| POST | `/api/v1/auth/register` | Registracija (rolė `User`). Grąžina žetonų porą. |
| POST | `/api/v1/auth/login` | Prisijungimas. Grąžina žetonų porą. |
| POST | `/api/v1/auth/refresh` | Atnaujinimo žetono keitimas nauja žetonų pora. |
| POST | `/api/v1/auth/logout` | Atsijungimas: panaikinami visi naudotojo atnaujinimo žetonai. |
| GET | `/api/v1/auth/me` | Prisijungusio naudotojo duomenys. |

- **Prieigos žetonas** – JWT, galioja 15 min., siunčiamas antraštėje `Authorization: Bearer <žetonas>`. Jame saugomi naudotojo ID (`sub`), vardas (`unique_name`), rolė (`role`), unikalus ID (`jti`), galiojimo laikas (`exp`), išdavėjas (`iss`) ir auditorija (`aud`). Pasirašomas HMAC-SHA256 algoritmu slaptu raktu `Jwt:Key`, kuris produkcinėje aplinkoje nustatomas aplinkos kintamuoju `Jwt__Key`.
- **Atnaujinimo žetonas** – atsitiktinė 64 baitų eilutė, galioja 7 d. Duomenų bazėje saugoma tik jo SHA-256 maiša. Kiekvieną kartą atnaujinant senas žetonas panaikinamas ir išduodamas naujas (rotacija). Jei panaudotas žetonas pateikiamas dar kartą, panaikinami visi to naudotojo žetonai.
- **Rolės** – svečias (be žetono), `User` ir `Admin`. Čempionus kurti, redaguoti ir šalinti gali tik `Admin`. Build’us ir komentarus kuria prisijungę naudotojai; redaguoti gali tik autorius, šalinti – autorius arba administratorius. Autorystė tikrinama serverio pusėje pagal žetone esantį naudotojo ID.
- **Atsako kodai** – `401`, kai žetono nėra, jis netinkamas ar pasibaigęs; `403`, kai naudotojas prisijungęs, bet neturi teisių.

Demonstraciniai naudotojai (slaptažodis `Password123!`): `admin` (Admin), `faker_fan`, `jungle_diff`, `support_main` (User).

Autorizacija tikrinama serverio pusėje pagal naudotojo rolę ir turinio autorystę. Grafinė sąsaja leidžia atlikti naudotojo teisėms priklausančias CRUD operacijas ir prisitaiko prie skirtingų ekrano dydžių.

## 4. Sistemos architektūra ir diegimas

Sistemą sudaro trys dalys:

1. **Kliento aplikacija** – React ir TypeScript grafinė sąsaja, vykdoma naudotojo naršyklėje.
2. **REST API** – ASP.NET Core serverio aplikacija, vykdanti užklausas, tikrinanti teises ir dirbanti su duomenų baze.
3. **Duomenų bazė** – SQLite, sauganti čempionus, build’us, komentarus, naudotojų paskyras ir autentifikacijos sesijų duomenis.

Kliento aplikacija ir REST API talpinamos debesijos platformoje Railway ir pasiekiamos internetu. Naršyklė gauna kliento aplikacijos failus iš frontend paslaugos, o aplikacija kreipiasi į REST API per HTTPS. Prie duomenų bazės jungiasi tik serverio aplikacija; naršyklė tiesioginės prieigos prie jos neturi.

Sistema patalpinta Railway platformoje (projektas `lol-builds`), kur abi dalys diegiamos automatiškai iš `main` šakos:

| Dalis | Adresas | Vykdymo aplinka |
| --- | --- | --- |
| Kliento aplikacija | https://lol-builds.up.railway.app | Railway paslauga `web`: sukompiliuoti React failai, aptarnaujami Caddy serverio |
| REST API | https://api-production-3b89593d.up.railway.app | Railway paslauga `api`: ASP.NET Core (.NET 10, Kestrel) |
| API dokumentacija | https://api-production-3b89593d.up.railway.app/swagger | Swagger UI |
| Duomenų bazė | – | SQLite failas `/data/lolbuilds.db` Railway tome (volume), prijungtame prie `api` paslaugos |

Produkcinėje aplinkoje konfigūracija perduodama aplinkos kintamaisiais: `Jwt__Key` (žetonų pasirašymo raktas), `ConnectionStrings__Default` (duomenų bazės kelias), `Cors__AllowedOrigins` (leidžiamas kliento aplikacijos adresas) ir `VITE_API_URL` (API adresas kliento aplikacijai).

UML diegimo diagrama pateikiama ataskaitoje.

## 5. Paleidimas

Reikia .NET 10 SDK ir Node.js.

```bash
# API
cd backend
dotnet run --urls http://localhost:5224

# Kliento aplikacija (kitame terminale)
cd frontend
npm install
npm run dev
```

Kliento aplikacija pasiekiama adresu `http://localhost:5173`. API adresas nurodomas aplinkos kintamuoju `VITE_API_URL` (numatytasis `http://localhost:5224`).

- Duomenų bazės failas `lolbuilds.db` sukuriamas ir užpildomas automatiškai. Norint atkurti pradinius duomenis, failą užtenka ištrinti.
- OpenAPI specifikacija ir Swagger UI: `http://localhost:5224/swagger`.
- Postman užklausų rinkinys su testais: `postman/LolBuilds.postman_collection.json`. Paleidimas: `postman collection run postman/LolBuilds.postman_collection.json`.
