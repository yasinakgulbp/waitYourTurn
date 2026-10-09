# Hayatta Kalma: fiyat ve zorluk çalışma notu — D45

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

Başlangıç **10000 test altını**; her Restart tekrar10000. İlk dalganın dört temel zombisi48 altın kazandırır. Bu gelirle fiyatlar tek dalgada erişilebilir değildir; ilk satın alma zamanı, sonraki mühimmat yenilemeleri ve onarım giderleri birlikte değerlendirilmeli. Turret500, her yenilemede tekrar500; silah yeniden satın alma aynı fiyata eksik mühimmatı tamamlar. Dron180 ve güncel-vagon onarımı40 önceki deneme değerleridir; kullanıcı yeni fiyat belirtmediği için değişmedi. Can yenileme kuralı mevcut canın%50'si, minimum30; maksimum canı aşmaz.

## Uygulanan ekipman

2026-10-09 hızlı deneme revizyonu: başlangıç parası 10000 oldu. Survival kataloğundaki SMG/tüfek/pompalı için de geçici requiresWeaponUnlock=false; para yeterliyken ana menü kilidi denemeyi engellemez. Kalıcı profil açılışları ve Battle kataloğu değişmez. Yayın ekonomisi hazırlanırken bu deneme ayarları kaldırılacak. Önceki D45 kabul raporu o günkü 3000 başlangıcının tarihsel kanıtıdır.

Bomba atar:90 alan hasarı,2.4m patlama yarıçapı,14m menzil,14m/s uçuş,0.10m süpürme yarıçapı,1.25s atış aralığı,6 şarjör+12 yedek,2.5s otomatik doldurma. İlk blokta düz temas patlaması; sekme veya balistik yay henüz yok. Her kare önceki/yeni konum arasında SphereCastNonAlloc ile kontrol edilir; ince metalden yavaş karede atlamaz. Mermi geçiren cam katmanı geçilir; metal pencere dikmeleri ve kapı alt paneli geçilmez. Fiziksel mermi yarıçapı dar açıklıkları kısıtlayabilir. Atış otomatik nişanın aynı görünürlük kuralını kullanır.16 sabit mermi/görsel yuvası; doluyken mermi harcanmaz. Görsel patlama hasar vermez.

Mayın:80 hasar,2m patlama,0.65m tetikleme mesafesi,0.6s kurma,0.1s düşman kontrolü. Mevcut12HP Intro ve70HP Tough'u kapaksız patlamada öldürür; gelecekte değişen HP için tek atış garantisi verilmez. Oyuncunun ayaklarına, açık iç zemine kurulur;0.5m birbirinden uzaklık, tren başına12 yuvayla sınırlıdır. Oyuncu/friendly tetiklemez ve hasar almaz; collider/NavMeshObstacle yok, üzerinden geçilir. Kırmızı gösterge materyali yanıp söner; gerçek Light yoktur. Metal arkasındaki düşman tetiklemez; patlama metal arkasına hasar vermez. İlk düşman temasında mayın tüketilir, bildirimlerden önce durumu kapatılır; aynı mayın iki kez patlamaz. Para öldürme sahibine mevcut KillRewards üzerinden gider.

Mayınlar konum/yuva/vagon/kurma süresiyle, uçan bombalar tanım/konum/yön/kalan menzil ve mevcut silah mühimmatıyla kaydolur. Yükleme ikinci mermi harcamaz veya ödül üretmez. Ölüm→yeni koşu bütün mayınları/uçuşları ve satın alınmış silahı sıfırlar. İçerik doğrulaması, yeni silah tanım alanları nedeniyle önceki aktif koşuları reddedebilir; kalıcı profil korunur. Battle sahnesi, kataloğu ve oynanış değerleri bu adımda değişmez. Bomba atar koşu ürünü olarak geçici **requiresWeaponUnlock=false** kullanır: mevcut dört silahlı profil şeması zorla değiştirilmez. Kalıcı açılış fiyatı/profil göçü ayrı sonraki iş.

## Önerilen sonraki zorluk sistemi — henüz uygulanmadı

Her istasyona elle bir satır yazmak yerine tek veriden, koşu tohumu+istasyonla yeniden üretilebilir bir dalga tarifi çıkarmak:

1. İlk1/2/3 istasyon için4/7/10 öğretici üretim korunur. Sonrası bir **tehdit bütçesi eğrisi** ile kontrol edilir.
2. Türün ayrı tehdit maliyeti olur: örneğin temel1, hızlı2, dayanıklı4. Bunlar altın ödülü değildir; HP, hız ve kapı hasarı birlikte düşünülerek ilk deneme değerleri seçilir.
3. Türlerin açıldığı istasyon, karışım ağırlığı ve dalga başına üst sınırı belirlenir. Bütçe kadar tür satın alan üretici bir defa istasyon başında mevcut SpawnStream dizisine çevirir; mevcut kapasite/kuyruk/üretim kotası motoru korunur. Her kare yeni plan, yeni düşman motoru veya sahne Find çağrısı eklenmez.
4. Toplam bütçe ile **tempo** ayrı ayarlardır: ilk geliş gecikmesi, geliş aralığı, kısa kümeler ve nefes araları. Hayatta Kalma yolculuğu zaten planlama/onarım arası sağlar. Canlı24 tren/8 vagon ve havuz12/vagon sert sınırları korunur; bütçe canlı sayısı demek değildir, fazlası kontrollü kuyruktadır.
5. Battle ve Survival ayrı eğri/tür karışımı kullanır; ortak plan üretici ve SpawnSchedule paylaşılır. Battle rekabetini sessizce kişiye göre değiştirmeyiz. İlk sürümde otomatik oyuncuya göre güç değiştirme eklenmez.

İstasyon başı kısa denge çıktısı: toplam tehdit, tür adetleri, en yüksek eşzamanlı canlı, dalga süresi, kazanılan/harcanan altın, kırılan kapı, oyuncu can kaybı, ilk turret/silah satın alma istasyonu. Az sayıda gerçek oynama ile bu birkaç eğriyi düzenleyebiliriz; hiçbir matematik tek başına keyif veya ticari başarıyı kanıtlamaz. Baştan gelen10000 test altını gerçek ekonomi değerlendirmesinde ayrıca çıkarılır.

Kaynak ve sınır: [Michael Booth / Valve, The AI Systems of Left 4 Dead, slayt77–91](https://cdn.fastly.steamstatic.com/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf) sürekli baskı yerine yoğun/sakin dönemleri ve Director'ın zorluk genliğinden ayrı tempo düzenlemesini anlatır. Yukarıdaki maliyetli dalga bütçesi bu tren oyununa bizim önerimizdir; Valve'ın bu tam bütçe formülünü kullandığı iddia edilmez. Büyük stüdyoların tümünün kullandığı tek bir garanti formül yoktur.

Sonraki işler: dalga bütçesi verisi+küçük istasyon tablosu, Battle'a ekipman/bot entegrasyonu, launcher kalıcı açılışı, kalıcı hızlı onarım, hedef Android'de yeni patlama/görsel yük kontrolü. Bu adımda telefon/ısı/pil kabulü yapılmadı.
