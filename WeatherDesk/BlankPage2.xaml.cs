using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Windows.Devices.Geolocation;
using Microsoft.UI.Composition;
using Microsoft.UI;
using Newtonsoft.Json;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace WeatherDesk
{
    public sealed partial class BlankPage2 : Page
    {
        private const string ApiKey = "7ac8ab2d149a434bacb133756243011";
        private const string WeatherApiUrl = "https://api.weatherapi.com/v1/forecast.json";
        public dynamic? data { get; private set; }
        public string CurrentLocation { get; private set; } = string.Empty;

        public BlankPage2()
        {
            this.InitializeComponent();
            this.Loaded += async (s, e) => await InitializeWeatherDataAsync();
            Add3DShadowToCard();
        }

        private async Task InitializeWeatherDataAsync()
        {
            try
            {
                var userLocation = await GetCurrentLocationAsync();
                if (userLocation.HasValue)
                {
                    CurrentLocation = $"{userLocation.Value.Latitude},{userLocation.Value.Longitude}";
                    await FetchWeatherDataAsync(CurrentLocation);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error: Unable to retrieve user location.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing weather data: {ex.Message}");
            }
        }

        private async Task<BasicGeoposition?> GetCurrentLocationAsync()
        {
            try
            {
                var geolocator = new Geolocator { DesiredAccuracyInMeters = 50 };
                Geoposition position = await geolocator.GetGeopositionAsync();
                return position?.Coordinate?.Point?.Position;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
                return null;
            }
        }

        private async Task FetchWeatherDataAsync(string location)
        {
            try
            {
                using HttpClient client = new HttpClient();
                string requestUrl = $"{WeatherApiUrl}?key={ApiKey}&q={location}&days=1&hourly=1";

                HttpResponseMessage response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Raw JSON: {jsonResponse}");

                data = JsonConvert.DeserializeObject(jsonResponse);

                

                if (data != null)
                {
                    var currentTemperature = (double?)data?.current?.temp_c ?? 0;
                    var currentPrecipitation = (double?)data?.current?.precip_mm ?? 0;
                    var maxTemperature = (double?)data?.forecast?.forecastday[0]?.day?.maxtemp_c ?? 0;
                    var minTemperature = (double?)data?.forecast?.forecastday[0]?.day?.mintemp_c ?? 0;

                    UpdateUI(currentTemperature, currentPrecipitation, maxTemperature, minTemperature);

                    int hoursCount = data?.forecast?.forecastday[0]?.hour?.Count ?? 0;
                    for (int i = 0; i < hoursCount; i++) // Corrected loop condition
                    {
                        var hourlyTemp = (double?)data?.forecast?.forecastday[0]?.hour?[i]?.temp_c ?? 0;

                        // Safely get hourly image icon, and ensure no errors if the icon is null
                        var hourlyImg = (string?)data?.forecast?.forecastday[0]?.hour?[i]?.condition?.icon;
                        hourlyImg = hourlyImg?.Replace("//", "https://") ?? ""; // Avoid null reference

                        // Safely get the time and ensure it's properly formatted
                        var hourlyTime = (string?)data?.forecast?.forecastday[0]?.hour?[i]?.time;
                        hourlyTime = hourlyTime?.Replace(DateTime.Now.ToString("yyyyMMdd"), "") ?? "";

                        // Call AddBox with the correct arguments
                        AddBox(hourlyTemp.ToString(), hourlyImg, hourlyTime, data, i);

                        wind.Text =
                                    "Wind mph: " + (((double?)data?.current?.wind_mph)?.ToString() ?? "") + "\n" +
                                    "Wind kph: " + (((double?)data?.current?.wind_kph)?.ToString() ?? "") + "\n" +
                                    "Wind degree: " + (((double?)data?.current?.wind_degree)?.ToString() ?? "") + "\n" +
                                    "Wind dir: " + (data?.current?.wind_dir ?? "");

                        SR.Text = (string?)data?.forecast?.forecastday[0]?.astro?.sunrise ?? "";
                        SS.Text = "Sunset: " + (((string?)data?.forecast?.forecastday[0]?.astro?.sunset ?? ""));
                        UvI.Text = "UV: " + (((double?)data?.current?.uv ?? 0).ToString());

                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error: Weather data is null.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching weather data: {ex.Message}");
            }
        }

        private void UpdateUI(double currentTemperature, double currentPrecipitation, double maxTemperature, double minTemperature)
        {
            try
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Temp.Text = $"{currentTemperature} °C";
                    PB.Text = $"{currentPrecipitation} mm";
                    MX.Text = $"Max: {maxTemperature} °C";
                    MN.Text = $"Min: {minTemperature} °C";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating UI: {ex.Message}");
            }
        }

        public void AddBox(string topText, string imageSource, string bottomText, dynamic data, int i)
        {
            // Check if the grid has fewer than 3 child elements
            if (f10w.Children.Count < 3)
            {
                // Create and display a text block if there are fewer than 3 items
                var CT = (string?)data?.current?.condition?.text;
                CT = CT?.ToLower() ?? "";
                if (CT.Contains("sunny") || CT.Contains("clear"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "A commonly recited dua for good weather is:\r\n\r\nاللهم إني أسألك خير هذه الرياح وخير ما فيها وخير ما أرسلت به، وأعوذ بك من شر هذه الرياح وشر ما فيها وشر ما أرسلت به.\r\n\r\nTransliteration:\r\n\"Allahumma inni as'aluka khayra hadhihi ar-riyah, wa khayra ma fiha, wa khayra ma ursilat bihi, wa a'udhu bika min sharri hadhihi ar-riyah, wa sharri ma fiha, wa sharri ma ursilat bihi.\"\r\n\r\nTranslation:\r\n\"O Allah, I ask You for the good of these winds, the good that is in them, and the good with which they were sent. And I seek refuge in You from the evil of these winds, the evil that is in them, and the evil with which they were sent.\"",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                } else if (CT.Contains("cloudy"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "Dua for Good Weather and Protection:\r\n\r\nاللهم إني أسالك خير هذه الريح وخير ما فيها وخير ما أرسلت به، وأعوذ بك من شر هذه الريح وشر ما فيها وشر ما أرسلت به\r\n\r\nTransliteration:\r\nAllahumma inni as’aluka khayra hadhihi ar-rih, wa khayra ma fiha, wa khayra ma ursilat bihi, wa a’udhu bika min sharri hadhihi ar-rih, wa sharri ma fiha, wa sharri ma ursilat bihi.\r\n\r\nTranslation:\r\nO Allah, I ask You for the good of this wind, the good it carries, and the good with which it has been sent, and I seek refuge in You from the evil of this wind, the evil it carries, and the evil with which it has been sent.\r\n\r\nThis dua can be recited during any weather event, including cloudy, windy, or stormy conditions.\r\n\r\nDua for Seeking Protection from Bad Weather:\r\nIf the weather turns threatening or stormy, you can also recite the following supplication:\r\n\r\nDua for Protection from Harmful Weather:\r\n\r\nاللهم إني أعوذ بك من شر ما يجلبه هذا الريح من شر وما تتركه من سوء\r\n\r\nTransliteration:\r\nAllahumma inni a’udhu bika min sharri ma yajlibuhu hadhihi ar-rih min sharri wa ma tatruku min su’i.\r\n\r\nTranslation:\r\nO Allah, I seek refuge in You from the evil that this wind brings, and from the harm it leaves behind.",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                } else if (CT.Contains("party cloudy"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "Dua for Good Weather and Protection:\r\nThis is a general supplication asking for good weather and protection from harm:\r\n\r\nاللهم إني أسالك خير هذه الريح وخير ما فيها وخير ما أرسلت به، وأعوذ بك من شرها وشر ما فيها وشر ما أرسلت به\r\n\r\nTransliteration: Allahumma inni as’alu ka khayra hathihi ar-reeh, wa khayra ma feeha, wa khayra ma ursilat bihi, wa a’oodhu bika min sharriha wa sharri ma feeha wa sharri ma ursilat bihi.\r\nTranslation: O Allah, I ask You for the good of this wind, the good of what it contains, and the good of what it has been sent with, and I seek refuge in You from the evil of it, the evil of what it contains, and the evil of what it has been sent with.\r\nDua for Protection from Harmful Weather:\r\nIf the weather appears to be harsh or threatening, this dua can be recited for protection:\r\n\r\nاللهم اجعلها صيفاً مباركاً علينا\r\n\r\nTransliteration: Allahumma ajil ha sayfan mubaarakan alayna.\r\nTranslation: O Allah, make it a blessed summer for us.",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                } else if (CT.Contains("rainy") || CT.Contains("rain"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "Dua for Rain:\r\n\r\nArabic: اللهم صِيبًا نافعًا Transliteration:\r\nAllahumma Sayyiban Naafi'a\r\nTranslation:\r\n\"O Allah, send down beneficial rain.\"",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                } else if (CT.Contains("thunderstorm"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "The Dua for Thunderstorm:\r\nWhen a thunderstorm occurs, the Prophet Muhammad (peace be upon him) used to recite the following:\r\n\r\nاللهم لا تقتلنا بغضبك، ولا تهلكنا بعذابك، وعافنا قبل ذلك\r\n\r\nTransliteration:\r\n\"Allahumma la tu'athibna bi ghadabika, wa la tuhlikna bi 'adhabika, wa 'aafina qabl thalik.\"\r\n\r\nTranslation:\r\n\"O Allah, do not destroy us with Your anger, and do not destroy us with Your punishment, and grant us safety before that.\"\r\n\r\nAdditional Prayers:\r\nDua for seeking protection from calamities: اللهم إني أسالك خير ما في هذه الريح، وخير ما فيها، وخير ما أرسلت به، وأعوذ بك من شر ما فيها، وشر ما أرسلت به\r\n\r\nTransliteration:\r\n\"Allahumma inni as’aluka khayr ma fi hadhihi ar-reeh, wa khayr ma feeha, wa khayr ma arsaltu bih, wa a'udhu bika min sharri ma feeha, wa sharri ma arsaltu bih.\"\r\n\r\nTranslation:\r\n\"O Allah, I ask You for the best of what this wind brings, the best of what is in it, and the best of what You have sent it with. And I seek refuge in You from the evil of what is in it, and the evil of what You have sent it with.\"",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                } else if (CT.Contains("snowy") || CT.Contains("snow"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "Dua for Rain (can be said during snow as well):\r\nاللهم صيّبًا نافعًا\r\nAllahumma Sayyiban Naafi'an\r\nO Allah, send down beneficial rain.\r\n\r\nThis is a supplication that seeks the mercy and blessings of Allah through rain (or snow). Snow, as a form of precipitation, can be included under this dua as well.\r\n\r\nAnother Dua for Weather:\r\nProphet Muhammad (PBUH) also taught the following dua to be recited when seeking Allah’s help during extreme weather conditions:\r\n\r\nاللهم إني أسالك من خير هذه الريح، وخير ما فيها، وخير ما أرسلت به، وأعوذ بك من شر هذه الريح، وشر ما فيها، وشر ما أرسلت به\r\nAllahumma inni as’alu ka min khayri hadhihi al-reeh, wa khayri ma feeha, wa khayri ma ursilat bih, wa a’oodhu bika min sharri hadhihi al-reeh, wa sharri ma feeha, wa sharri ma ursilat bih\r\nO Allah, I ask You for the good of this wind (or snow), the good that is in it, and the good with which it has been sent. And I seek refuge with You from the evil of this wind (or snow), the evil that is in it, and the evil with which it has been sent.\r\n\r\nGeneral Advice:\r\nGratitude and Patience: Snow, like any form of weather, is a blessing from Allah. It's essential to show gratitude and seek refuge from any harm caused by it.\r\nDu'a for Protection: During cold or snowy weather, Muslims can make general supplications for protection, well-being, and safety, like asking Allah to keep them warm and shielded from the harms of the weather.",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                } else if (CT.Contains("foggy"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "Dua for protection from harm:\r\n\r\nاللهم إني أعوذ بك من الهم والحزن والعجز والكسل والبخل والجبن ومن عذاب القبر ومن فتنة المحيا والممات\r\n\r\nTransliteration:\r\nAllahumma inni a'udhu bika min al-hammi wal-huzni wal-'ajzi wal-kasali wal-bukhli wal-jubni wa min 'adhab al-qabri wa min fitnat al-mihya wal-mamat.\r\n\r\nTranslation:\r\n\"O Allah, I seek refuge with You from anxiety and grief, from incapacity and laziness, from miserliness and cowardice, and from the punishment of the grave, and from the trials of life and death.\"\r\n\r\nThis dua can be recited in any situation where one feels vulnerable, including during foggy or potentially hazardous weather.\r\n\r\nAdditionally, it is common to make general duas asking Allah for safety, protection, and mercy in any circumstance. Here is a simple dua for safety:\r\n\r\nاللهم احفظني من كل سوء\r\n\r\nTransliteration:\r\nAllahumma hifdhni min kulli su'\r\n\r\nTranslation:\r\n\"O Allah, protect me from all harm.\"\r\n\r\nIn the case of fog, since it can impair visibility and pose a danger while traveling, it is recommended to make a general dua for protection and to take necessary precautions for safety. The Prophet Muhammad (PBUH) also recommended reciting specific supplications during travel, such as:\r\n\r\nبِسْمِ اللَّهِ تَوَكَّلْتُ عَلَى اللَّهِ وَلَا حَوْلَ وَلَا قُوَّةَ إِلَّا بِاللَّهِ\r\n\r\nTransliteration:\r\nBismillah, tawakkaltu ‘ala Allah, wa la hawla wa la quwwata illa billah.\r\n\r\nTranslation:\r\n\"In the name of Allah, I place my trust in Allah, and there is no power and no strength except with Allah.\"",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                } else if (CT.Contains("windy"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "1. Dua for Protection from Harmful Winds\r\nIt is recommended to recite the following dua for protection from harmful or destructive winds:\r\n\r\nاللّهُمَّ اجْعَلْهَا رِيحًا طَيِّبَةً (Allahumma ajilaha reehaan tayyibatan)\r\n\r\nTranslation: \"O Allah, make this wind a beneficial wind.\"\r\n\r\n2. Dua for Good Weather\r\nYou can also recite this dua for seeking good weather:\r\n\r\nاللّهُمَّ صَيِّبًا نَافِعًا (Allahumma sayyiban naafi'an)\r\n\r\nTranslation: \"O Allah, make it a beneficial rain.\"\r\n\r\nThis dua is usually said during rain, but it can also be a general prayer for good, beneficial weather.\r\n\r\n3. General Duas for Seeking Allah's Protection\r\nAnother general dua that can be said for protection from any adverse weather or natural disasters, including strong winds:\r\n\r\nاللّهُمَّ إِنِّي أَعُوذُ بِكَ مِنْ زَلاَزِلِ وَالْجَانِّ وَالْجَذَامِ وَالْجُنُونِ وَالْبَرَصِ وَمِنْ سَيِّئَاتِ الْأَمْرَاضِ وَالْمَوَاتِ (Allahumma inni a'udhu bika min zalaazil, wal jaani, wal jadhami, wal junooni, wal barasi, wa min sayyi'ati al-amraadi wal mawaati)\r\n\r\nTranslation: \"O Allah, I seek refuge with You from earthquakes, jinn, leprosy, madness, blindness, and the evil of diseases and death.\"",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                } else if (CT.Contains("dusty"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "Dua for Dusty Weather:\r\nWhen the weather is dusty, the Prophet Muhammad (peace be upon him) recommended seeking refuge from harmful conditions. The dua you can say during such times is:\r\n\r\nاللهم إني أسالك خير هذه الريح وخير ما فيها وخير ما أمرت به، وأعوذ بك من شر هذه الريح وشر ما فيها وشر ما أمرت به.\r\n\r\nTransliteration: \"Allahumma inni as'aluka khayra hathihi ar-reeh, wa khayra ma fiha, wa khayra ma umirat bih, wa a'udhu bika min sharri hathihi ar-reeh, wa sharri ma fiha, wa sharri ma umirat bih.\"\r\n\r\nTranslation: \"O Allah, I ask You for the good of this wind, the good of what is in it, and the good of what You have commanded with it. And I seek refuge in You from the evil of this wind, the evil of what is in it, and the evil of what You have commanded with it.\"",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                }if (CT.Contains("tornado"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "Dua for Protection from Harm:\r\n\r\nاللهم إني أعوذ بك من شر ما عملت ومن شر ما لم أعمل\r\nAllahumma inni a'udhu bika min sharri ma 'amiltu wa min sharri ma lam a'mal\r\nTranslation: \"O Allah, I seek refuge in You from the evil of what I have done and the evil of what I have not done.\"\r\nDua for Protection from Natural Disasters:\r\n\r\nاللهم إنا نسالك من فضلك ورحمتك، اللهم إنا نسالك أن ترفع عنا البلاء\r\nAllahumma inna nas'alu ka min fadlika wa rahmatika, Allahumma inna nas'alu ka an tarfa'a 'anna al-balaa\r\nTranslation: \"O Allah, we ask You for Your grace and mercy, O Allah, we ask You to lift the calamity from us.\"\r\nDua for Protection from Any Harmful Event:\r\n\r\nبِسْمِ اللَّهِ الَّذِي لَا يَضُرُّ مَعَ اسْمِهِ شَيْءٌ فِي الْأَرْضِ وَلَا فِي السَّمَاءِ وَهُوَ السَّمِيعُ الْعَلِيمُ\r\nBismillahilladhi la yadurru ma'asmihi shay'un fi al-ardi wa la fi as-samaa'i wa huwa as-samee'u al-aleem\r\nTranslation: \"In the name of Allah, with whose name nothing on earth or in the heavens can harm, and He is the All-Hearing, All-Knowing.\"\r\nNote: This dua is commonly recited for protection from any harm, including natural disasters.\r\nGeneral Dua for Protection:\r\n\r\nاللهم أستودعك نفسي وأهلي وأموالي من كل شر\r\nAllahumma astawdi'uka nafsi wa ahli wa amwali min kulli shar\r\nTranslation: \"O Allah, I place in Your care my soul, my family, and my wealth from all evil.\"",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                }if (CT.Contains("hurricane"))
                {
                    var infoTextBlock = new TextBlock
                    {
                        Text = "1. General Supplication for Protection:\r\nDu'a for seeking protection from calamities:\r\n\"اللهم إني أعوذ بك من الهم والحزن، والعجز والكسل، والبخل والجبن، وغلبة الدين، وقهر الرجال.\"\r\nTransliteration: \"Allahumma inni a’udhu bika min al-hammi wal-hazan, wal-‘ajzi wal-kasal, wal-bukhli wal-jubni, waghallabatid-dayni waqahri-rijal.\"\r\nTranslation: \"O Allah, I seek refuge in You from anxiety and sorrow, from helplessness and laziness, from miserliness and cowardice, from the burden of debts and from the tyranny of men.\"\r\n2. Du'a for Protection from Natural Disasters:\r\nProphet Muhammad (PBUH) said: \"اللهم إني أسالك خيرها وخير ما فيها وخير ما أرسلت به، وأعوذ بك من شرها وشر ما فيها وشر ما أرسلت به.\"\r\nTransliteration: \"Allahumma inni as'aluka khayraha wa khayra ma feeha wa khayra ma ursilat bihi, wa a'udhu bika min sharriha wa sharri ma feeha wa sharri ma ursilat bihi.\"\r\nTranslation: \"O Allah, I ask You for the good in it, and the good that is in it, and the good that it was sent with. And I seek refuge in You from its evil, and from the evil that is in it, and from the evil it was sent with.\"\r\n3. Du'a for Protection and Safety:\r\nGeneral supplication for protection:\r\n\"أعوذ بكلمات الله التامات من شر ما خلق.\"\r\nTransliteration: \"A'udhu bi kalimatillahi at-tammati min sharri ma khalaq.\"\r\nTranslation: \"I seek refuge with the perfect words of Allah from the evil of what He has created.\"\r\n4. Du'a for Relief from Hardships:\r\nDu'a for relief from hardship:\r\n\"لا إله إلا أنت سبحانك إني كنت من الظالمين.\"\r\nTransliteration: \"La ilaha illa Anta, Subhanaka inni kuntu min az-zalimin.\"\r\nTranslation: \"There is no deity but You, Glory is to You, indeed I have been of the wrongdoers.\"\r\n5. Du'a for All Calamities:\r\nDu'a for seeking refuge from all forms of calamities:\r\n\"اللهم لا تقتلنا بغضبك، ولا تهلكنا بعذابك، وعافنا قبل ذلك.\"\r\nTransliteration: \"Allahumma la taqtulna bighadabika, wa la tuhlikna bi’adhaabika, wa ‘aafina qabl dhalik.\"\r\nTranslation: \"O Allah, do not kill us with Your anger, and do not destroy us with Your punishment, and grant us safety before that.\"",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 16,
                        Margin = new Thickness(10)
                    };

                    // Clear previous content and add the info text to the grid
                    f10w.Children.Clear();
                    f10w.Children.Add(infoTextBlock);
                }
            }
            else if (f10w.Children.Count < 10)
            {
                // Limit to 10 boxes
                var hourlyTemp = (double?)data?.forecast?.forecastday[0]?.hour?[i]?.temp_c ?? 0;

                // Safely get hourly image icon, and ensure no errors if the icon is null
                var hourlyImg = (string?)data?.forecast?.forecastday[0]?.hour?[i]?.condition?.icon;
                hourlyImg = hourlyImg?.Replace("//", "https://") ?? ""; // Avoid null reference

                // Safely get the time and ensure it's properly formatted
                var hourlyTime = (string?)data?.forecast?.forecastday[0]?.hour?[i]?.time;
                hourlyTime = hourlyTime?.Replace(DateTime.Now.ToString("yyyyMMdd"), "") ?? "";

                // Create a new Box (can be a Button, TextBlock, or any UI element)
                var box = new Border
                {
                    Width = 100,
                    Height = 150, // Increased height to accommodate top text, image, and bottom text
                    Background = new SolidColorBrush(Colors.Transparent),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                // Create a StackPanel to stack elements vertically inside the Box
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Top Text
                var topTextBlock = new TextBlock
                {
                    Text = topText,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 10, 0, 0) // Adding margin for spacing
                };

                // Middle Image
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(imageSource, UriKind.RelativeOrAbsolute)),
                    Width = 50, // Adjust image size
                    Height = 50,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Bottom Text
                var bottomTextBlock = new TextBlock
                {
                    Text = bottomText,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 10, 0, 0) // Adding margin for spacing
                };

                // Add the text and image to the StackPanel
                stackPanel.Children.Add(topTextBlock);
                stackPanel.Children.Add(image);
                stackPanel.Children.Add(bottomTextBlock);

                // Set the StackPanel as the content of the box
                box.Child = stackPanel;

                // Add the box to the Grid
                f10w.Children.Add(box);

                // Optionally, update layout
                f10w.InvalidateArrange();
            }
        }


        private void Add3DShadowToCard()
        {
            try
            {
                var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
                List<SpriteVisual> shadowVisuals = new List<SpriteVisual>();

                for (int i = 0; i < 10; i++)
                {
                    var shadow = compositor.CreateDropShadow();
                    shadow.Color = Colors.Black;
                    shadow.Opacity = 0.1f + (i * 0.05f);
                    shadow.BlurRadius = 15f + (i * 5f);
                    shadow.Offset = new System.Numerics.Vector3(0, (i + 1) * 8, 0);

                    var shadowVisual = compositor.CreateSpriteVisual();
                    shadowVisual.Shadow = shadow;
                    shadowVisual.Size = new System.Numerics.Vector2((float)CardContainer.ActualWidth, (float)CardContainer.ActualHeight);

                    shadowVisuals.Add(shadowVisual);
                    ElementCompositionPreview.SetElementChildVisual(ShadowCanvas, shadowVisual);
                }

                CardContainer.SizeChanged += (s, e) =>
                {
                    foreach (var shadowVisual in shadowVisuals)
                    {
                        shadowVisual.Size = new System.Numerics.Vector2((float)CardContainer.ActualWidth, (float)CardContainer.ActualHeight);
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding 3D shadow: {ex.Message}");
            }
        }
    }
}
