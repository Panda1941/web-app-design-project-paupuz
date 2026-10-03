# LoL Builds

Saityno taikomųjų programų projektavimo modulio projektas – „League of Legends“ čempionų build’ų kūrimo ir aptarimo platforma.

Šiame dokumente pateikiama projekto užduotis ir numatomas sistemos veikimas. Aprašytos funkcijos dar nėra realizuotos.

## 1. Sprendžiamo uždavinio aprašymas

### 1.1. Sistemos paskirtis

„League of Legends“ žaidėjai tam pačiam čempionui gali naudoti skirtingus daiktus (items), burtus (summoner spells) ir runas (runes), priklausomai nuo žaidimo pozicijos bei pasirinktos strategijos. Norint pasidalinti tokiu pasirinkimu, reikia pateikti ne tik patį build’ą, bet ir paaiškinti, kada ir kaip jį naudoti.

Projekto tikslas – sukurti internetinę platformą, kurioje naudotojai galėtų kurti čempionams skirtus build’us, dalintis jais ir aptarti juos komentaruose. Build’ą sudarys pavadinimas, aprašymas, žaidimo pozicija, pasirinkti daiktai, summoner spell'ai ir runos.

Čempionai bus iš anksto įkelti į sistemą, o jų informaciją valdys administratorius. Build’us ir komentarus kurs registruoti naudotojai. Neprisijungę lankytojai galės peržiūrėti viešai pateiktą turinį.

### 1.2. Taikomosios srities objektai

Sistemoje numatomi trys pagrindiniai objektai:

| Objektas | Paskirtis ir pagrindinė informacija |
| --- | --- |
| **Čempionas (Champion)** | Žaidimo čempionas, kuriam kuriami build’ai. Saugomas jo pavadinimas, titulas, aprašymas ir paveikslėlio nuoroda. |
| **Build’as (Build)** | Konkrečiam čempionui skirtas naudotojo sukurtas build’as. Saugomas pavadinimas, aprašymas, žaidimo pozicija, pasirinkti daiktai, burtai, runos, autorius ir sukūrimo bei atnaujinimo laikas. |
| **Komentaras (Comment)** | Naudotojo atsiliepimas arba klausimas po konkrečiu build’u. Saugomas tekstas, autorius ir sukūrimo bei atnaujinimo laikas. |

Objektai sudaro hierarchiją **Champion → Build → Comment**:

- Vienam čempionui gali priklausyti daug build’ų. Kiekvienas build’as priklauso vienam čempionui.
- Vienam build’ui gali priklausyti daug komentarų. Kiekvienas komentaras priklauso vienam build’ui.

Naudotojų paskyros bus saugomos atskirai ir naudojamos autentifikacijai, autorizacijai bei turinio autorystei nustatyti. Jos nėra vienas iš trijų pagrindinių taikomosios srities objektų.

Daiktai, burtai ir runos bus pasirenkami iš iš anksto parengto katalogo. Pasirinkta kombinacija bus saugoma kaip build’o dalis; atskiros jų CRUD valdymo funkcijos į pradinę projekto apimtį neįtraukiamos.

### 1.3. Funkciniai reikalavimai

#### Svečias

Neprisijungęs naudotojas galės:

1. Peržiūrėti čempionų sąrašą ir konkretaus čempiono informaciją.
2. Peržiūrėti pasirinktam čempionui sukurtus build’us.
3. Peržiūrėti konkretaus build’o aprašymą, daiktus, burtus ir runas.
4. Peržiūrėti build’o komentarus.
5. Užsiregistruoti ir prisijungti prie sistemos.

#### Registruotas naudotojas

Prisijungęs naudotojas galės naudotis svečiui prieinamomis turinio peržiūros funkcijomis ir papildomai:

1. Sukurti pasirinktam čempionui skirtą build’ą: nurodyti jo pavadinimą, aprašymą ir žaidimo poziciją, pasirinkti daiktus, burtus bei runas.
2. Redaguoti ir pašalinti savo sukurtus build’us.
3. Rašyti komentarus po build’ais.
4. Redaguoti ir pašalinti savo komentarus.
5. Atsijungti nuo sistemos.

Registruotas naudotojas negalės redaguoti ar pašalinti kitų naudotojų build’ų ir komentarų. Šios teisės bus tikrinamos serverio pusėje pagal prisijungusio naudotojo identifikatorių.

#### Administratorius

Administratorius galės naudotis registruotam naudotojui prieinamomis funkcijomis ir papildomai:

1. Pridėti naują čempioną.
2. Redaguoti čempiono informaciją.
3. Pašalinti čempioną.
4. Pašalinti netinkamus build’us ir komentarus, nepriklausomai nuo jų autoriaus.

## 2. Numatomos REST API operacijos

Kiekvienam pagrindiniam objektui bus realizuotos penkios operacijos: sukūrimas, vieno objekto gavimas, atnaujinimas, pašalinimas ir sąrašo gavimas. Iš viso numatoma **15 taikomosios srities API metodų**. Registracijos, prisijungimo, atsijungimo ir žetonų atnaujinimo metodai bus papildomi.

| Objektas | Metodas | Kelias | Paskirtis |
| --- | --- | --- | --- |
| Champion | GET | `/api/champions` | Gauti čempionų sąrašą. |
| Champion | POST | `/api/champions` | Sukurti čempioną. |
| Champion | GET | `/api/champions/{championId}` | Gauti konkretų čempioną. |
| Champion | PUT | `/api/champions/{championId}` | Atnaujinti čempioną. |
| Champion | DELETE | `/api/champions/{championId}` | Pašalinti čempioną. |
| Build | GET | `/api/champions/{championId}/builds` | Gauti konkretaus čempiono build’ų sąrašą. |
| Build | POST | `/api/champions/{championId}/builds` | Sukurti build’ą konkrečiam čempionui. |
| Build | GET | `/api/champions/{championId}/builds/{buildId}` | Gauti konkretų čempiono build’ą. |
| Build | PUT | `/api/champions/{championId}/builds/{buildId}` | Atnaujinti build’ą. |
| Build | DELETE | `/api/champions/{championId}/builds/{buildId}` | Pašalinti build’ą. |
| Comment | GET | `/api/champions/{championId}/builds/{buildId}/comments` | Gauti konkretaus build’o komentarų sąrašą. |
| Comment | POST | `/api/champions/{championId}/builds/{buildId}/comments` | Sukurti komentarą po konkrečiu build’u. |
| Comment | GET | `/api/champions/{championId}/builds/{buildId}/comments/{commentId}` | Gauti konkretų build’o komentarą. |
| Comment | PUT | `/api/champions/{championId}/builds/{buildId}/comments/{commentId}` | Atnaujinti komentarą. |
| Comment | DELETE | `/api/champions/{championId}/builds/{buildId}/comments/{commentId}` | Pašalinti komentarą. |

Komentarų keliai apima visus tris hierarchijos objektus. Serveris tikrins, ar build’as priklauso nurodytam čempionui, o komentaras – nurodytam build’ui.

API grąžins duomenis JSON formatu ir užklausos rezultatą atitinkančius HTTP būsenos kodus. Visiems prieigos taškams bus parengtas užklausų rinkinys testavimui, o kiekvieno pagrindinio objekto lentelė bus užpildyta bent penkiais prasmingais įrašais.

## 3. Pasirinktos ir numatomos technologijos

Frontend ir backend technologijos pasirinktos atsižvelgiant į patirtį su kitais projektais. Duomenų bazei ir prieigai prie jos kol kas siūlomi PostgreSQL ir Entity Framework Core.

| Sistemos dalis | Technologija | Pasirinkimo pagrindimas |
| --- | --- | --- |
| Kliento dalis | **React ir TypeScript** | React komponentai leis kurti ir pakartotinai naudoti čempionų, build’ų bei komentarų sąsajos elementus. TypeScript padės aprašyti API duomenų struktūras ir aptikti tipų klaidas kuriant programą. |
| Serverio dalis | **C# ir ASP.NET Core** | Bus naudojami REST API, duomenų tikrinimui, autentifikacijai ir autorizacijai realizuoti. Šios technologijos jau naudojamos kituose projektuose. |
| Duomenų bazė | **PostgreSQL** | Reliacinė duomenų bazė tinka čempionų, build’ų, komentarų ir naudotojų ryšiams saugoti bei duomenų vientisumui užtikrinti. |
| Prieiga prie duomenų bazės | **Entity Framework Core** | Leis aprašyti duomenų modelį C# klasėmis, vykdyti užklausas ir valdyti schemos pakeitimus migracijomis. PostgreSQL integracijai numatomas Npgsql teikėjas. |

Autentifikacijai numatoma naudoti JWT. Prieigos žetonai galios trumpai, o ilgesniam prisijungimo palaikymui bus naudojami atnaujinimo žetonai. Atnaujinant prieigą, atnaujinimo žetonas bus pakeičiamas nauju, o panaudotas žetonas taps nebegaliojantis. Atsijungus, sesijos žetonai turės būti panaikinami arba pažymimi nebegaliojančiais.

Autorizacija bus tikrinama serverio pusėje pagal naudotojo rolę ir turinio autorystę. Grafinė sąsaja leis atlikti naudotojo teisėms priklausančias CRUD operacijas ir prisitaikys prie skirtingų ekrano dydžių.

## 4. Sistemos architektūra ir diegimas

Numatomos trys sistemos dalys:

1. **Kliento aplikacija** – React ir TypeScript grafinė sąsaja, vykdoma naudotojo naršyklėje.
2. **REST API** – ASP.NET Core serverio aplikacija, vykdanti užklausas, tikrinanti teises ir dirbanti su duomenų baze.
3. **Duomenų bazė** – PostgreSQL, sauganti čempionus, build’us, komentarus, naudotojų paskyras ir autentifikacijos sesijų duomenis.

Kliento aplikacija ir REST API bus talpinamos debesijos aplinkoje ir pasiekiamos internetu. Naršyklė gaus kliento aplikacijos failus iš frontend talpinimo paslaugos, o aplikacija kreipsis į REST API per HTTPS. Prie duomenų bazės jungsis serverio aplikacija; naršyklė tiesioginės prieigos prie jos neturės.

**Diegimo diagrama bus pridėta vėliau.** Ji bus parengta pagal UML standartą, pasirinkus debesijos tiekėją ir konkretų sistemos dalių talpinimo būdą. Diagramoje bus parodytas naudotojo įrenginys su naršykle, frontend talpinimo aplinka, API vykdymo aplinka, duomenų bazė ir jų ryšiai.
