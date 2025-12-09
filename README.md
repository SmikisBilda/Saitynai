# Projekto „Prieigos taškai“ Aprašymas

## 1. Sprendžiamo Uždavinio Aprašymas

### 1.1. Sistemos Paskirtis

Projekto tikslas – palengvinti informacijos rinkimą apie prieigos taškus, kurie bus panaudoti vietinėje navigacijos sistemoje.

**Veikimo principas:**  
Sistemą sudaro dvi dalys:

- Internetinė aplikacija (naudojama neregistruotų vartotojų, registruotų vartotojų ir administratorių)
- Aplikacijų programavimo sąsaja (API)

**Vartotojų galimybės:**

- **Neregistruoti vartotojai** – gali tik peržiūrėti informaciją.
- **Registruoti vartotojai** – gali pridėti ir redaguoti informaciją apie jiems priskirtus skanavimo taškus.
- **Administratoriai** – tvirtina registracijas, valdo pastatus, jų planus, skanavimo taškus ir priskiria juos vartotojams.

---

### 1.2. Funkciniai Reikalavimai

#### Neregistruotas naudotojas gali:

1. Peržiūrėti informaciją apie prieigos taškus  
2. Prisijungti prie sistemos  

#### Registruotas naudotojas gali:

1. Atsijungti nuo sistemos  
2. Užsiregistruoti sistemoje  
3. Redaguoti naudotojui priskirtų skanavimo taškų informaciją  
4. Pridėti informaciją apie priskirtus skanavimo taškus  

#### Administratorius gali:

1. Patvirtinti vartotojo registraciją  
2. Atmesti vartotojo registraciją  
3. Pridėti pastatą  
4. Ištrinti pastatą  
5. Pridėti pastato planus  
6. Ištrinti pastato planus  
7. Pridėti skanavimo taškus  
8. Ištrinti skanavimo taškus  
9. Ištrinti konkretaus skanavimo taško prieigos taškų informaciją  
10. Priskirti naudotojui skanavimo taškus


## 2. Paleidimo instrukcijos
Programos reikalingos paleisti projektą:
- Git
- Docker

Komandos reikalingos paleisti projektą:
```
git clone https://github.com/SmikisBilda/Saitynai/
cd Saitynai
docker-compose build
docker-compose up -d
```
Programa turėtu būti pasiekiama per:
```
http://localhost:8080/swagger
```
## 3.	Naudotojo sąsajos projektas

<img width="845" height="738" alt="image" src="https://github.com/user-attachments/assets/d3380cd9-bd01-4d56-886e-587bd077c6d4" />
<img width="687" height="533" alt="image" src="https://github.com/user-attachments/assets/ee9f0ebb-b423-44fd-97f0-a87cb3addfda" />
<img width="845" height="727" alt="image" src="https://github.com/user-attachments/assets/5a4f9be5-eaf8-4c49-9f64-28c391bbb3ea" />
<img width="1022" height="638" alt="image" src="https://github.com/user-attachments/assets/53ae1b81-a3ad-411e-ba67-0a80d07b966f" />
<img width="843" height="745" alt="image" src="https://github.com/user-attachments/assets/28f62b77-f881-4da2-968f-6c66e7adef1a" />
<img width="679" height="648" alt="image" src="https://github.com/user-attachments/assets/e42f75ab-054d-4272-9ee8-7c36274bb74b" />







## 4.	OpenAPI specifikacija
https://github.com/SmikisBilda/Saitynai/blob/main/api.yaml
## 5.	Projekto išvados
1.	Įgyta patirtis kuriant pilnos apimties saityno projektus, apimančius serverio ir kliento dalies logiką.
2.	Išmokta kurti standartus atitinkančius API metodus ir ruošti išsamią techninę dokumentaciją.
3.	Įsisavinti JWT autentifikacijos principai ir jų taikymo metodai.
4.	Įgyti praktiniai gebėjimai diegti projektus į viešą tinklą naudojant „AWS“.


