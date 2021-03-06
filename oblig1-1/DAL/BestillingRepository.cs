﻿using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oblig1_1.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using Serilog;

namespace oblig1_1.DAL
{
    public class BestillingRepository : IBestillingRepository
    {
        private readonly BestillingContext _db;

        public BestillingRepository(BestillingContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<List<Bestillinger>> Index()
        {
            try
            {
                List<Bestillinger> alleBestillinger = await _db.Bestillinger.Select(best => new Bestillinger
                {
                    ID = best.ID,
                    Kunde = best.Kunde,
                    Pris = best.Pris,
                    Tur = best.Tur,
                    Retur = best.Retur
                }).ToListAsync();
                return alleBestillinger;
            }
            catch (Exception e)
            {
                Log.Error("Error i Index: {error}", e);
                return null;
            }
        }

        public async Task<List<RuteAvgang>> VisAlleRuteAvganger()
        {
            List<RuteAvgang> ruteavganger = await _db.RuteAvganger.ToListAsync();
            return ruteavganger;

        }
        private bool SammeDato(DateTime dato1, DateTime dato2) 
        {
            Console.WriteLine(dato1 + ", " + dato2);
            return dato1.Year == dato2.Year && dato1.Month == dato2.Month && dato1.Day == dato2.Day;
        }

        //Returnere en liste med ruteavganger 
        public List<RuteAvgang> FinnEnRuteAvgang(string[] holdeplasserOgDato) //kan ikke være async pga where
        {
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                Holdeplass fra = JsonSerializer.Deserialize<Holdeplass>(holdeplasserOgDato[0], serializerOptions);
                Holdeplass til = JsonSerializer.Deserialize<Holdeplass>(holdeplasserOgDato[1], serializerOptions);
                Console.WriteLine(fra.ToString() + ", " + til.ToString());
                DateTime date = DateTime.ParseExact(holdeplasserOgDato[2], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                Console.WriteLine(date);
                List<RuteAvgang> ruteavganger = new List<RuteAvgang>();
                List<Rute> potensielleRuter = new List<Rute>();
                //1.Finne rutestopp der holdeplassID tilsvarer holdeplass fraID

                //2.Loope rutestopplisten, inni loopen så leter vi etter rutestopp med samme ruteID, som har holdeplassID tilsvarende tilID
                //rekkefølgenr større enn fraID sitt rekkefølgenr
                //3.Hvis vi finner en eller flere, legger dette til i listen av rutekandidater
                foreach (var fraStopp in _db.Rutestopp.Where(r => r.Holdeplass.ID == fra.ID))
                {
                    foreach (var tilStopp in _db.Rutestopp.Where(r => r.Holdeplass.ID == til.ID && 
                                                                fraStopp.Rute == r.Rute))
                    {
                        potensielleRuter.Add(fraStopp.Rute);
                    }
                            /*if (stopp.Holdeplass.ID == til.ID || stopp.Holdeplass.ID>til.ID) {
                        potensielleRuter.Add(stopp.Rute);
                    }*/
                }
                if(potensielleRuter.Count > 0)
                {
                    potensielleRuter.ForEach(pr =>
                    {
                        Console.WriteLine("En mulig rute er: " + pr.Navn);
                    });
                }
                else
                {
                    Console.WriteLine("Ingen potensielle ruter :(");
                }
                //4.Looper listen av rutekandidater og finner ruteavganger som bruker ruta
                //5. Hvis ruteavgangen har riktig dato, legger den til i listen over ruteavganger
                
                foreach (var rute in potensielleRuter) {
                    foreach(var ruteavgang in _db.RuteAvganger.Where(ra => ra.Rute.RID == rute.RID))
                    {
                        ruteavganger.Add(ruteavgang);
                    }
                        
                }
                return ruteavganger;
            }
            catch (Exception e)
            {
                Log.Error("Error i SammeDato: {error}", e);
                return null;
            }
        }


        public async Task<bool> Slett(int id)
        {
            try
            {
                Bestillinger enBestilling = await _db.Bestillinger.FindAsync(id);
                _db.Bestillinger.Remove(enBestilling);
                await _db.SaveChangesAsync();
                Log.Information("Bestilling slettet.");
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Error i Slett: {error}", e);
                return false;
            }
        }

        public async Task<Bestillinger> HentEn(int id)
        {
            try
            {
                Console.WriteLine(id);
                Bestillinger enBestilling = await _db.Bestillinger.FindAsync(id);
                if (enBestilling == null) return null; //finner ikke id i DB
                var hentetBestilling = new Bestillinger()
                {
                    ID = enBestilling.ID,
                    Kunde = enBestilling.Kunde,
                    Pris = enBestilling.Pris,
                    Tur = enBestilling.Tur,
                    Retur = enBestilling.Retur
                };
                return hentetBestilling;
            }
            catch (Exception e)
            {
                Log.Error("Error i HentEn: {error}", e);
                Debug.WriteLine(e.Message);
                return null;
            }
            
        }

        public async Task<bool> Endre(Bestillinger endreBestilling)
        {
            try
            {
                Bestillinger enBestillling = await _db.Bestillinger.FindAsync(endreBestilling.ID);
                enBestillling.Kunde = endreBestilling.Kunde;
                enBestillling.Pris = endreBestilling.Pris;
                enBestillling.Tur = endreBestilling.Tur;
                enBestillling.Retur = endreBestilling.Retur;

                await _db.SaveChangesAsync();
                Log.Information("Bestilling endret.");
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Error i Endre: {error}", e);
                return false;
            }
        }

        public async Task<List<Holdeplass>> HentAlleHoldeplasser()
        {
            List<Holdeplass> holdeplasser = await _db.Holdeplasser.ToListAsync();
            return holdeplasser;
        }

        public static byte[] Hashing(string passord, byte[] salt)
        {
            return KeyDerivation.Pbkdf2(
                                password: passord,
                                salt: salt,
                                prf: KeyDerivationPrf.HMACSHA512,
                                iterationCount: 1000,
                                numBytesRequested: 32);
        }

        public static byte[] Salt()
        {
            var cryptoSP = new RNGCryptoServiceProvider();
            var salt = new byte[24];
            cryptoSP.GetBytes(salt);
            return salt;
        }

        public async Task<bool> LoggInn(Bruker bruker)
        {
            try
            {
                Brukere funnetBruker = await _db.Brukere.FirstOrDefaultAsync(b => b.Brukernavn == bruker.Brukernavn);
                if (funnetBruker == null) return false;
                // sjekker om passordet til bruker er riktig 
                byte[] hash = Hashing(bruker.Passord, funnetBruker.Salt);
                bool ok = hash.SequenceEqual(funnetBruker.Passord);
                if(ok)
                {
                    Log.Information("Admin logget inn.");
                    return true;
                }
                return false;
            }
            catch(Exception e)
            {
                Log.Error("Error i LoggInn: {error}", e);
                return false; 
            }
        }

        public async Task<Holdeplass> HentHoldeplass(int id)
        {
            try
            {
                Holdeplass enHoldeplass = await _db.Holdeplasser.FindAsync(id);
                var hentetHold = new Holdeplass()
                {
                    ID = enHoldeplass.ID,
                    Sted = enHoldeplass.Sted,
                    Sone = enHoldeplass.Sone
                };
                return hentetHold;
            }
            catch(Exception e)
            {
                Log.Error("Error i HentHoldeplass: {error}", e);
                return null;
            }
        }

        public async Task<bool> EndreHoldeplass(Holdeplass endreHoldeplass)
        {
            try
            {
                var enHoldeplass = await _db.Holdeplasser.FindAsync(endreHoldeplass.ID);
                enHoldeplass.Sted = endreHoldeplass.Sted;
                enHoldeplass.Sone = endreHoldeplass.Sone;

                await _db.SaveChangesAsync();
                Log.Information("Holdeplass id: {id} endret.", endreHoldeplass.ID);
                return true;
            }
            catch(Exception e)
            {
                Log.Error("Error i EndreHoldeplass: {error}", e);
                return false; 
            }
        }

        public async Task<bool> LagreHoldeplass(Holdeplass innHP)
        {
            try
            {
                var nyHS = new Holdeplass();
                nyHS.Sted = innHP.Sted;
                nyHS.Sone = innHP.Sone;

                _db.Holdeplasser.Add(nyHS);
                await _db.SaveChangesAsync();
                Log.Information("Holdeplass lagt inn: {holdeplass}", innHP.Sted);
                return true; 
            }
            catch(Exception e)
            {
                Log.Error("Error i LagreHoldeplass: {error}", e);
                return false; 
            }
        }

        public async Task<List<RuteStopp>> HentRuteStopp()
        {
            List<RuteStopp> alleRuteStopp = await _db.Rutestopp.ToListAsync();
            return alleRuteStopp;
        }

        public async Task<RuteStopp> EtRuteStopp(int id)
        {
            try
            {
                RuteStopp etRS = await _db.Rutestopp.FindAsync(id);

                Holdeplass holdeplass = await _db.Holdeplasser.FindAsync(etRS.Holdeplass.ID);

                var hentetRS = new RuteStopp()
                {
                    ID = etRS.ID,
                    StoppTid = etRS.StoppTid,
                    Holdeplass = holdeplass
                };
                return hentetRS;
            }
            catch(Exception e)
            {
                Log.Error("Error i EtRuteStopp: {error}", e);
                return null;
            }
        }

        public async Task<bool> SlettRS(int id)
        {
            try
            {
                RuteStopp etRS = await _db.Rutestopp.FindAsync(id);
                _db.Rutestopp.Remove(etRS);
                await _db.SaveChangesAsync();
                Log.Information("Rutestopp id: {id} slettet.", id);
                return true; 
            }
            catch(Exception e)
            {
                Log.Error("Error i SLettRS: {error}", e);
                return false; 
            }
        }

        public async Task<bool> EndreRS(RuteStopp endreRS)
        {
            try
            {
                var etRS = await _db.Rutestopp.FindAsync(endreRS.ID);
                if (!etRS.Holdeplass.Sted.Equals(endreRS.Holdeplass.Sted))
                {
                    var sjekkHoldeplass = _db.Holdeplasser.Where(s => s.Sted.Contains(etRS.Holdeplass.Sted));
                    if (sjekkHoldeplass == null)
                    {
                        var holdeplassRad = new Holdeplass();
                        holdeplassRad.Sted = endreRS.Holdeplass.Sted;
                        holdeplassRad.Sone = endreRS.Holdeplass.Sone;
                        etRS.Holdeplass = holdeplassRad;
                    }
                    else
                    {
                        etRS.Holdeplass = endreRS.Holdeplass;
                    }
                }
                etRS.StoppTid = endreRS.StoppTid;

                await _db.SaveChangesAsync();
                Log.Information("Rutestopp id: {id} endret.", endreRS.ID);
                return true;
            }
            catch(Exception e)
            {
                Log.Error("Error i EndreRS: {error}", e);
                return false;
            }
        }
        public RuteStopp NyttRuteStopp(string[] argumenter)
        {
            Console.WriteLine(argumenter[0]);
            Console.WriteLine(argumenter[1]);
            Console.WriteLine(argumenter[2]);
            string holdeplassNavn = argumenter[0];
            string ruteNavn = argumenter[1];
            int minutterEtterAvgang = int.Parse(argumenter[2]);
            TimeSpan stoppTid = TimeSpan.FromMinutes(minutterEtterAvgang);
            

            Holdeplass holdeplass = _db.Holdeplasser.Where(h => h.Sted == holdeplassNavn).FirstOrDefault();
            Rute rute = _db.Ruter.Where(r => r.Navn == ruteNavn).FirstOrDefault();
            if (holdeplass != null && rute != null)
            {
                RuteStopp nyttRuteStopp = new RuteStopp();
                nyttRuteStopp.Rute = rute;
                nyttRuteStopp.Holdeplass = holdeplass;
                nyttRuteStopp.StoppTid = stoppTid;
                _db.Rutestopp.Add(nyttRuteStopp);
                _db.SaveChanges();
                return nyttRuteStopp;
            }
            Log.Information("Nytt rutestopp lagt til.");
            return null;
        }
        public RuteAvgang NyRuteAvgang(string[] argumenter)
        {
            string ruteNavn = argumenter[0];
            string avgangsTidString = argumenter[1];
            DateTime avgangsTid = DateTime.ParseExact(avgangsTidString, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            Rute rute = _db.Ruter.Where(r => r.Navn == ruteNavn).FirstOrDefault();
            if (rute != null)
            {
                RuteAvgang nyRuteAvgang = new RuteAvgang();
                nyRuteAvgang.Rute = rute;
                nyRuteAvgang.Dato = avgangsTid;
                _db.RuteAvganger.Add(nyRuteAvgang);
                _db.SaveChanges();
                return nyRuteAvgang;
            }
            Log.Information("Ny ruteavgang lagt til.");
            return null;
        }

        public async Task<bool> LagreRS(RuteStopp innRS)
        {
            try
            {
                var nyRS = new RuteStopp();
                nyRS.StoppTid = innRS.StoppTid;

                // sjekker om holdeplass allerede ligger i databasen, legger til ny dersom den ikke gjør det 

                var sjekkHoldeplass = _db.Holdeplasser.Where(navn => navn.Sted.Contains(innRS.Holdeplass.Sted));
                if (sjekkHoldeplass == null)
                {
                    // oppretter en ny holdeplass 
                    var nyHoldeplass = new Holdeplass();
                    nyHoldeplass.Sted = innRS.Holdeplass.Sted;
                    nyHoldeplass.Sone = innRS.Holdeplass.Sone;
                    nyRS.Holdeplass = nyHoldeplass;
                }
                else
                {
                    nyRS.Holdeplass.Sted = innRS.Holdeplass.Sted;
                    nyRS.Holdeplass.Sone = innRS.Holdeplass.Sone;
                }

                _db.Rutestopp.Add(nyRS);
                await _db.SaveChangesAsync();
                Log.Information("Rutestopp lagret.");
                return true; 
            }
            catch(Exception e)
            {
                Log.Error("Error i LagreRS: {error}", e);
                return false; 
            }
        }

        public async Task<bool> SlettHoldeplass(int id)
        {
            try
            {
                Holdeplass enHoldeplass = await _db.Holdeplasser.FindAsync(id);
                _db.Holdeplasser.Remove(enHoldeplass);
                await _db.SaveChangesAsync();
            }
            catch(Exception e)
            {
                Log.Error("Error i SlettHoldeplass: {error}", e);
                Console.Write(e.Message);
                return false;
            }
            Log.Information("Holdeplass slettet.");
            return true;
        }

        public async Task<List<Rute>> AlleRuter()
        {
            try
            {
                List<Rute> alleRuter = await _db.Ruter.Select(r => new Rute
                {
                    RID = r.RID,
                    Navn = r.Navn,
                    RuteStopp = r.RuteStopp
                }).ToListAsync();
                return alleRuter;
            }
            catch(Exception e)
            {
                Log.Error("Error i AlleRuter: {error}", e);
                return null;
            }
        }

        public async Task<Rute> EnRute(int id)
        {
            try
            {
                Rute enRute = await _db.Ruter.FindAsync(id);
                var hentetRute = new Rute()
                {
                    RID = enRute.RID,
                    Navn = enRute.Navn,
                    RuteStopp = enRute.RuteStopp
                };
                return hentetRute;
            }
            catch(Exception e)
            {
                Log.Error("Error i EnRute: {error}", e);
                return null;
            }
        }

        // lagrer en tom rute
        public async Task<bool> LagreRute(string navn)
        {
            try
            {
                var nyRute = new Rute();
                nyRute.Navn = navn;
                List<RuteStopp> tom = new List<RuteStopp>();
                nyRute.RuteStopp = tom;

                _db.Ruter.Add(nyRute);
                await _db.SaveChangesAsync();
                Log.Information("Rute lagret.");
                return true; 
            }
            catch(Exception e)
            {
                Log.Error("Error i LagreRute: {error}", e);
                return false; 
            }
        }

        public async Task<List<Priser>> HentPriser()
        {
            List<Priser> priser = await _db.Priser.ToListAsync();
            return priser;
        }

        public async Task<Priser> EnPris(int id)
        {
            try
            {
                Priser enPris = await _db.Priser.FindAsync(id);
                var returPris = new Priser()
                {
                    PrisID = enPris.PrisID,
                    Prisklasse = enPris.Prisklasse,
                    Pris1Sone = enPris.Pris1Sone,
                    Pris2Sone = enPris.Pris2Sone,
                    Pris3Sone = enPris.Pris3Sone,
                    Pris4Sone = enPris.Pris4Sone
                };
                return returPris;
            }
            catch (Exception e)
            {
                Log.Error("Error i EnPris: {error}", e);
                return null;
            }
            
        }
        

        public async Task<bool> EndrePriser(Priser pris)
        {
            try
            {
                var endreObjekt = await _db.Priser.FindAsync(pris.PrisID);

                endreObjekt.Pris1Sone = pris.Pris1Sone;
                endreObjekt.Pris2Sone = pris.Pris2Sone;
                endreObjekt.Pris3Sone = pris.Pris3Sone;
                endreObjekt.Pris4Sone = pris.Pris4Sone;
                await _db.SaveChangesAsync();
                Log.Information("Priser endret.");
            }
            catch (Exception e)
            {
                Log.Error("Error i EndrePriser: {error}", e);
                return false;
            }
            return true;
        }
    }
}
