# Hayatta Kalma: fiyat, mühimmat ve zorluk çalışma notu — D46

Bu değerler 2026-10-09 kullanıcının deneme kararlarıdır; yayın dengesi değildir. Battle kataloğu/bot harcamaları bu adımda değişmez. Bomba atar ve mayın ortak Combat/Defenses uygulamasıdır; Battle sahnesine ve bot davranışlarına ekleme sonraki Battle adımıdır.

| Koşu ürünü | Altın | 12 altınlık temel zombi karşılığı, yukarı yuvarlanmış |
|---|---:|---:|
| Can yenileme | 150 | 13 |
| Tahta, tüm dış kapılar | 600 | 50 |
| Tel örgü, tahtadan sonra | 1200 | 100 |
| Normal turret | 500 | 42 |
| Gelişmiş turret | 900 | 75 |
| Hafif makineli | 300 | 25 |
| Pompalı | 1000 | 84 |
| Tüfek | 2500 | 209 |
| Bomba atar | 6000 | 500 |
| Mayın | 250 | 21 |

Başlangıç **10000 test altını**; her Restart tekrar10000. İlk dalganın dört temel zombisi48 altın kazandırır. Bu gelirle fiyatlar tek dalgada erişilebilir değildir; ilk satın alma zamanı, sonraki mühimmat yenilemeleri ve onarım giderleri birlikte değerlendirilmeli. Turret500, her yenilemede tekrar500. Survival'da silah koşuda bir kere alınır; daha sonra ayrı ucuz mühimmat ürünü kullanılır. Dron180 ve güncel-vagon onarımı40 önceki deneme değerleridir; kullanıcı yeni fiyat belirtmediği için değişmedi. Can yenileme kuralı mevcut canın%50'si, minimum30; maksimum canı aşmaz.

## D46 — mühimmat ve hareketli hedef

Kullanıcı daha kısa pompalı/bomba atar menzili, hareketli hedef için daha iyi nişan, daha çok yedek ve ucuz şarjör istedi. Aşağıdakiler ilk deneme ayarlarıdır; şarjör fiyatları mühendislik seçimidir.

| Silah | Menzil (m) | Dolu şarjör | Yedek mermi | Toplam | Bir yedek şarjör fiyatı |
|---|---:|---:|---:|---:|---:|
| SMG | 10 | 24 | 168 | 192 | 50 |
| Rifle | 14 | 20 | 140 | 160 | 300 |
| Shotgun | 7 | 6 | 42 | 48 | 125 |
| Grenade launcher | 12 | 6 | 42 | 48 | 750 |

Tabanca sınırsız yedek kullanır. Diğer dört silah dolu şarjör +7 yedek şarjörle alınır. Mağazadaki `Supplies / Ammo` bölümünden sahip olunan silaha bir şarjör eklenebilir. Şarjör ve yedek tamamen bittiğinde ekranda `Reload / $ fiyat` çıkar. İşlem mevcut Wallet/ShopService üzerinden tek teslimle yapılır; normal dolum süresi korunur, anında ateş sağlamaz. Yedek kapasitesi başlangıç stoğunu aşmaz; son kısmi boşlukta en çok eksik miktar verilir. Sahip olunmayan silahın veya dolu yedeğin mühimmatı alınamaz. Mermi kaydı mevcut WeaponSnapshot ile sürer.

Bomba hızı14→24m/s. AutoAim yalnız seçilmiş hedefin ardışık konumlarını örnekler; ProjectileAim sabit hızla kesişim noktasını hesaplar. Ulaşılmaz tahmin, teleport veya tahmin yönünde metal varsa doğrudan nişana döner. Bomba atıldıktan sonra hedefe dönmez; ani yön değiştiren düşmanı hâlâ ıskalayabilir. Cam/metal ve temas patlaması mevcut resolver üzerinden korunur. Atış hazır olduğunda tahmin hattında ek bir süpürme yapılır; sahne araması veya atış başına nesne üretimi eklenmez.

Silah tanımları ortaktır: menzil/hız/başlangıç stoğu bu assetleri kullanan Battle oyuncusuna da uygulanır. **Ucuz şarjör ürünleri ve tek satın alma kuralı yalnız SurvivalShop'a bağlandı.** Battle'ın eski tekrar satın alarak tamamlama kuralı, bot harcamaları ve spawn programları korunur; ucuz mühimmat/bot entegrasyonu sonraki Battle işidir. Turret/dron mühimmatı değişmez.

## Uygulanan ekipman

2026-10-09 hızlı deneme revizyonu: başlangıç parası 10000 oldu. Survival kataloğundaki SMG/tüfek/pompalı için de geçici requiresWeaponUnlock=false; para yeterliyken ana menü kilidi denemeyi engellemez. Kalıcı profil açılışları ve Battle kataloğu değişmez. Yayın ekonomisi hazırlanırken bu deneme ayarları kaldırılacak. Önceki D45 kabul raporu o günkü 3000 başlangıcının tarihsel kanıtıdır.

Bomba atar:90 alan hasarı,2.4m patlama yarıçapı,12m menzil,24m/s uçuş,0.10m süpürme yarıçapı,1.25s atış aralığı,6 şarjör+42 yedek,2.5s otomatik doldurma. İlk blokta düz temas patlaması; sekme veya balistik yay henüz yok. Her kare önceki/yeni konum arasında SphereCastNonAlloc ile kontrol edilir; ince metalden yavaş karede atlamaz. Mermi geçiren cam katmanı geçilir; metal pencere dikmeleri ve kapı alt paneli geçilmez. Fiziksel mermi yarıçapı dar açıklıkları kısıtlayabilir. Atış otomatik nişanın aynı görünürlük kuralını kullanır.16 sabit mermi/görsel yuvası; doluyken mermi harcanmaz. Görsel patlama hasar vermez.

Mayın:80 hasar,2m patlama,0.65m tetikleme mesafesi,0.6s kurma,0.1s düşman kontrolü. Mevcut12HP Intro ve70HP Tough'u kapaksız patlamada öldürür; gelecekte değişen HP için tek atış garantisi verilmez. Oyuncunun ayaklarına, açık iç zemine kurulur;0.5m birbirinden uzaklık, tren başına12 yuvayla sınırlıdır. Oyuncu/friendly tetiklemez ve hasar almaz; collider/NavMeshObstacle yok, üzerinden geçilir. Kırmızı gösterge materyali yanıp söner; gerçek Light yoktur. Metal arkasındaki düşman tetiklemez; patlama metal arkasına hasar vermez. İlk düşman temasında mayın tüketilir, bildirimlerden önce durumu kapatılır; aynı mayın iki kez patlamaz. Para öldürme sahibine mevcut KillRewards üzerinden gider.

Mayınlar konum/yuva/vagon/kurma süresiyle, uçan bombalar tanım/konum/yön/kalan menzil ve mevcut silah mühimmatıyla kaydolur. Yükleme ikinci mermi harcamaz veya ödül üretmez. Ölüm→yeni koşu bütün mayınları/uçuşları ve satın alınmış silahı sıfırlar. İçerik doğrulaması, yeni silah tanımları ve program nedeniyle önceki aktif koşuları reddedebilir; kalıcı profil korunur. Bomba atar koşu ürünü olarak geçici **requiresWeaponUnlock=false** kullanır: mevcut dört silahlı profil şeması zorla değiştirilmez. Kalıcı açılış fiyatı/profil göçü ayrı sonraki iş.

## D46 — uygulanan tehdit bütçesi

Her istasyona elle bir satır yazmak yerine `Survival04.asset` tek veri programından istasyon başında dalga üretilir. Aynı istasyon ve içerik aynı tarifi verir; bu ilk sürümde tohuma göre karışım rastgeleleştirilmez.

1. İlk1/2/3 istasyon için4/7/10 öğretici üretim korunur. Dördüncü istasyon bütçesi13; sonraki her istasyonda+3, en fazla160. Formül `min(160, 13 + (istasyon-4)*3)`. Dördüncü dalga13 temel zombidir; sonraki bütçe zombi sayısıyla aynı değildir.
2. Temel maliyet1/ağırlık6/başlangıç4; hızlı maliyet2/ağırlık2/başlangıç5; dayanıklı maliyet4/ağırlık1/başlangıç8. Bunlar altın ödülü değildir. Karışım maliyeti bütçeyi aşmaz. Tür başı dalga sınırları120/40/20.
3. Ağırlıklı üretici bir defa istasyon başında mevcut SpawnStream dizisine çevirir. **Tek tren bütçesi üç vagona bölünür**, vagon başına tekrar çoğaltılmaz. Başlangıçta4s gecikme, tür/vagon akışında2.5s aralık ve vagonlar arasında0.35s kaydırma vardır; aynı anda gelen toplam baskı tek akıştan yüksek olabilir. Bunlar Inspector ayarıdır.
4. Toplam bütçe ile tempo ayrı ayarlardır. Hayatta Kalma yolculuğu planlama/onarım arası sağlar. Canlı24 tren/8 vagon, havuz12/vagon, kuyruk32, karede2 istek ve4 ısıtma sınırları korunur; bütçe canlı sayısı değildir. Yeni kalabalığı sınırsız Instantiate ederek zorlaştırmayız. Dalga planlama sırasında en fazla1000 tehdit birimi kabul edilir.
5. Ortak StationDefinition/SpawnSchedule kullanılır; bütçe isteğe bağlıdır. Battle programlarında kapalı, Battle eğrisi değişmez. Otomatik oyuncuya göre güç değiştirme yoktur. İçerik anahtarı maliyet/ağırlık/açılma/sınır/eğri değişimini algılar; kayıttan aynı dalga yeniden kurulur, farklı içerikte eski koşu yüklenmez.

Kurulum: SurvivalIntegration açıkken, Play kapalıyken `Wait Your Turn → Survival → Apply Ammo and Difficulty` (Ctrl+Alt+U). Mevcut nav/geometriyi tekrar üretmez; yalnız katalog/program bağlarını kaydeder. Yeni Survival sahnesi builder'ı aynı içeriği kullanır. Eski `Survival06` asset'i bu sahnenin program listesinde değildir.

İstasyon başı kısa denge çıktısı: toplam tehdit, tür adetleri, en yüksek eşzamanlı canlı, dalga süresi, kazanılan/harcanan altın, kırılan kapı, oyuncu can kaybı, ilk turret/silah satın alma istasyonu. Az sayıda gerçek oynama ile bu birkaç eğriyi düzenleyebiliriz; hiçbir matematik tek başına keyif veya ticari başarıyı kanıtlamaz. Baştan gelen10000 test altını gerçek ekonomi değerlendirmesinde ayrıca çıkarılır.

Kaynak ve sınır: [Michael Booth / Valve, The AI Systems of Left 4 Dead, slayt77–91](https://cdn.fastly.steamstatic.com/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf) sürekli baskı yerine yoğun/sakin dönemleri ve Director'ın zorluk genliğinden ayrı tempo düzenlemesini anlatır. Yukarıdaki maliyetli dalga bütçesi bu tren oyununa bizim önerimizdir; Valve'ın bu tam bütçe formülünü kullandığı iddia edilmez. Büyük stüdyoların tümünün kullandığı tek bir garanti formül yoktur.

PC kabulü:38 silah +45 Run/spawn/yolculuk +13 ekonomi testi PASS; boş launcher'a750 altınla6 yedek/normal reload, tam fiyatlı tekrar silah alımının reddi, mayın/bomba hasar-ödül-kayıt ve yeni koşu temizliği kısa Play kontrolünde PASS. Mağaza mühimmat kartları görsel olarak kontrol edildi; Console0 hata. Kanıt `generated/survival-ammo-aim-budget-pc-acceptance.txt`.

Sonraki işler: kısa gerçek oynama ile tür/tempo/gelir dengesi, mevcut MODEL_CONTRACT ölçülerine göre Blender tren giydirme, Battle'a patlayıcı ekipman/ucuz mühimmat/bot entegrasyonu, launcher kalıcı açılışı, kalıcı hızlı onarım, hedef Android'de yeni patlama/görsel yük kontrolü. Bu adımda telefon/ısı/pil kabulü yapılmadı.
