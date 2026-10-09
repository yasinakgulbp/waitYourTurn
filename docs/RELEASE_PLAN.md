# Çalışan temelden yayınlanabilir oyuna

Güncelleme: 2026-10-09. Bu belge ROADMAP içindeki M8/M9'u somutlaştırır; yeni bir oyun mimarisi kurma işi değildir. Uygulanan kod, önerilen sonraki iş ve doğrulanmamış ürün varsayımı ayrı tutulur.

**Kullanıcı kararı:** İlk yayında Battle ve Solo eşit önemdedir. İkisi de ilk deneyim, kayıt/devam, sonuç ve denge kabulünden geçer; biri diğerinin yerine test edilmez. Battle yerel bot yarışı; gerçek multiplayer henüz kapsamda değildir. Solo botsuz daha uzak istasyona ilerleme deneyimidir; tamamlanmış hikâye içeriği ayrıca tasarlanacaktır. IAP ürünleri ve kalıcı ekonomi hâlâ seçilmedi; bu belge onları kesinleştirmez.

Battle süresi: kullanıcı önce 5–8 dakika seçti, ardından emin olmadığını ve yaşayanların devam edebilmesini istedi. **Zorunlu toplam süre sınırı eklenmez.** D33 eleme/birincilik kuralı korunur; istasyon süresi yarışın toplam süresi değildir. 5–8 dakika yalnız geçici tempo inceleme referansıdır, kabul şartı veya zorla bitiş kararı değildir. Gerçek koşu dağılımı oynanış testinde değerlendirilir.

## Şimdiki durum ve eksikler

Mekanik temel PC'de çalışır: iki taraftan saldırı, 4–6 kapı, cam/metal atışı, onarım, silahlar, sınırlı spawn, turret/dron, bot/eleme ve yerel kayıt. D35 oyuncu sınırı/yürüyüş düzeltmesi de PC'de geçti. Bunun kanıtı kullanıcının keyfi, dış oyuncunun geri dönmesi veya ticari başarı değildir.

Güncel birleşik sahne A54'te ölçülmedi. Android arka plan/kill/devam, dokunma ergonomisi, gerçek uzun savaş ve ısınma açık. Kamera hâlâ `RunPresentation` tarafından doğrudan yerleştiriliyor; olay bazlı hasar/patlama hissi ve oyun sesi bu yeni runtime içinde kurulmuş değil. HUD/mağaza/sonuçlar geliştirici IMGUI sunumuna dayanıyor; üretim menüsü/öğretici ve erişilebilir geri bildirim eksik. Başlangıçtaki 1000 para deneme ayarıdır. Üç profil, dört silah ve rastgele vagon, tek başına uzun süreli içerik veya tekrar oynama gerekçesi olarak kabul edilmez. Kalıcı ödül/harcama amacı, IAP ve reklam servisleri açık; sonuç çarpanı için teslim edilecek kalıcı ödül henüz tasarlanmamıştır.

## Sıra ve kabul kapıları

| Adım | Somut iş | Sonraki adıma geçme koşulu |
| --- | --- | --- |
| M8b — Cihaz temeli | Güncel TrainIntegration için ayrı test APK'sı; Battle ve Solo dokunma, mağaza, kapı onarımı, kayıt/arka plan/süreç kapatma/geri açma. Savaş ve savunmalar açıkken kare süreleri/bellek gözlemi. | İki modda ilerleme korunur, ölüm/sonuç eski koşuyu diriltmez; UI erişilebilir; hata ve başlangıç ölçümü kayıtlıdır. Eski lab APK'sı bu kabul yerine geçmez. |
| M8c — Oynama hissi ve kullanıcı akışı | Hafif kamera takibi, sınırlı hasar/patlama tepkisi; isabet, kapı tehlikesi, onarım, reload, boş mermi ve satın alma geri bildirimi. Temel sesler; sade başla/mod/devam/sonuç/tekrar ekranı ve kısa bağlamsal öğretici. | Efekt açık/kapalı cihaz karşılaştırması; iki taraftaki kapılar/zombiler okunur; aynı dokunma niyetinde hareket/atış değişmez. Battle/Solo ilk giriş anlaşılır; efektleri azaltma/kapatma seçeneği çalışır. |
| M8d — İlk ürün değerlendirmesi | Önce ilk 3–5 istasyonda tempo, para/onarım/silah/turret/dron kararları; sonra küçük dış oyuncu grubu. Battle ve Solo ayrı gözlem. | Oyuncular açıklama olmadan temel döngüyü anlayabilir; tekrar deneme isteği ve çıkış/ölüm sebepleri kaydedilir. Sorun varsa önce kontrol/denge/gerilim düzeltilir; yeni özellik eklemek varsayılan çözüm değildir. |
| M9a — Tek vagon sanat örneği | Bir vagon, istasyon parçası, oyuncu/zombi modeli, bir silah, turret/dron görseli ve temel ses/ışıkla hedef görünümü kanıtla. Materyal/render yolu ve model bütçesini bu küçük örnekte kesinleştir. | MODEL_CONTRACT pivot/ölçü/cam/metal/çarpışma/namlu kuralları korunur; aynı sahne cihazda oynanabilir ve ölçülmüş bütçeye sığar. Kullanıcı görünümü kabul eder; sonra diğer modeller üretilir. |
| M9b — İçerik ve uzun oturum | Kabul edilen sanat yaklaşımını varyantlara genişlet; iki modun farklı hedef/tempo/bot/zorluk eğrilerini dengele. Sade üretim UI, ses/VFX sınırları, kalite profilleri ve görünmeyen vagon maliyetini ölçüme göre azalt. | Düşük/orta cihaz ve farklı en-boy oranında 15–30 dakika gerçek savaş; artan bellek/ısı/sıkışma yok veya düzeltmesi kayıtta. Model değişimi mekanik kabulünü bozmaz. |
| M9c — Gelir ve ölçüm | Kullanıcıyla kalıcı ödül/harcama amacını ve IAP ürünlerini seç. Unity Ads tercihiyle sağlayıcı adaptörü, Google Play IAP; tek teslim, hata/offline, reklam kaldırma/geri yükleme. Gerekli ölçüm ve gizlilik akışı. | Sahte servis testleri ve resmi test ortamında teslim doğrulanır; satın alma/reklam zorunlu ilerleme engeli olmaz. Sonuç/ödül kimliğiyle çift teslim engellenir; temel deneyim reklamsız da çalışır. |
| M9d — Kapalı test ve sınırlı yayın | Güncel Play gereklilikleri, mağaza görselleri, gerçek oynanış videosu; iki modda kapalı test ve ardından kullanıcı tarafından onaylanan sınırlı yayın/pazarlama denemesi. | Gerçek cohort verisi: geri dönüş, terk/ölüm noktaları, teknik kalite, reklam sonrası davranış ve gelir. Reklam maliyeti/gelir aynı ülke/kanal/süreyle kıyaslanır. Test bütçesi ve durdurma sınırı önceden seçilir. |
| M9e — Genel yayın ve büyütme | Kritik sorunları kapat; mağaza, destek ve izleme hazır olsun. Pazarlama harcamasını veriyle artır. | Ölçülen oyuncu davranışı ve net birim ekonomi büyütmeyi destekler. Mağazada öne çıkarılma gelir planının zorunlu varsayımı değildir. |

Bu aşamalar sabit gün/hafta tahmini değildir. Her adım başında küçük çıktı, kapsam sınırı ve deneme bütçesi belirlenir; ölçüm çıkmadan sonraki büyük üretim açılmaz. Ek hikâye içeriği, yeni vagon sayıları, geniş yetenek ağacı, kombo ve sezon sistemi bu kapıların şartı değildir; mevcut iki mod ilk yayın hedefi olarak korunur.

## Kamera: şimdi ne, sonra ne?

**Önce M8b mevcut oyun ölçümü, sonra M8c hafif geri bildirim, sonra aynı cihazda karşılaştırma.** Kamera hissi modellemeden önce denenir; nihai renk/ışık/alan derinliği sanat örneğiyle değerlendirilir. Cihaz hazırlığı bu plan çalışmasına paralel ilerleyebilir.

İlk deneme: yumuşak/sınırlı takip, çok küçük konum sarsıntısı, kısa hasar kenar geri bildirimi ve mesafeye/şiddete göre sınırlı patlama tepkisi. Görüntü titremesi her mermi ve her zombi saldırısında üst üste büyümez; en güçlü darbeler birleştirilir, eşzamanlı etki ve süre bütçesi vardır. Ateş/isabet/kapı kırılması ve onarım sesi ile okunur animasyon en az kamera kadar önemlidir. Sürekli bulanıklık, motion blur ve ağır tam ekran efektler başlangıç varsayılanı değildir.

`RunPresentation` tek kamera konum sahibidir. Efekt modülü yalnız sunum isteği sağlar; mevcut kamera üzerine ikinci bağımsız takip scripti/Cinemachine sürücüsü bindirilmez. Ana kadraj hesabı ile geçici görsel ofset ayrıdır; safe-area ve iki platformun korunmuş hacmi sarsıntı sırasında da kadrajda kalır. Hasar/ölüm/onarım/silah olaylarını gözler; gameplay ona bağımlı olmaz. Süreler karanlıkta ilerlemez, yeniden koşu/yükleme/vagon atamasında eski darbe temizlenir; görsel kuyruğu kayıt verisi değildir.

**Kritik mevcut bağımlılık:** `PlayerMotor`, ekran hareketini kameranın yatay forward/right eksenlerinden hesaplar. Kameraya döndürme sarsıntısı eklenirse joystick/WASD yönü de oynayabilir. İlk deneme konum ofseti kullanır; yön/roll efekti gerekirse kontrol eksenleri önce nominal kamera yönünden ayrı okunur ve karşılaştırmalı test yapılır. Görsel efekt hedef/mermi/nav/collider konumlarını taşımaz.

Mevcut proje Built-in render yolundadır (`GraphicsSettings` render pipeline boş); URP/post-processing paketi kurulmuş değil. Sırf bulanıklık için bugün pipeline geçişi yapılmaz. Bulanıklık kapıları veya istasyon yaklaşımını gizliyorsa kullanılmaz. Görsel derinlik önce kadraj, renk/değer ayrımı, aydınlatma ve ölçülü çevre detaylarıyla kurulur; pahalı alan derinliği ancak ölçülmüş kalite seçeneği olarak değerlendirilir.

## Performans yaklaşımı

“Hiçbir telefonda hiç düşüş olmaz” doğrulanabilir bir hedef değildir. Desteklenecek cihaz sınıfı ve kalite profili seçilir; A54 tek başına düşük cihaz/tablet kanıtı sayılmaz. Başlangıç mühendislik hedefi A54'te kararlı 60 FPS; düşük profilde kararlı 30 FPS seçeneği ve isteğe bağlı efekt kapatma. Bunlar ölçülmüş sonuç veya bütün cihazlarda garanti değildir.

Kare süresi dağılımı, uzun takılmalar, CPU/GPU, bellek/GC, ısı ve oturum süresi birlikte kaydedilir. Efekt açık/kapalı aynı savaş yükünde karşılaştırılır. Model/animasyon/kapı, turret/dron, parçacıklar, ses, gölge ve UI aynı bütçeye dahildir; yalnız boş sahnedeki FPS kabul değildir. Development/Profiler teşhis içindir; nihai kalite yayın benzeri build'de de doğrulanır. Erken ölçüm kısa tutulabilir, M9'da 15–30 dakikalık ısınma/uzun oturum kontrolü yapılır.

## Eğlence ve para kazanmayı nasıl sınayacağız?

Bu oyunun test edilecek ana vaadi: **kapılar kırılmadan dışarıyı vurmak, onarım/konum/harcama kararlarıyla baskıyı yönetmek ve farklı vagon koşullarına uyum sağlamak.** Battle buna yarış/eleme, Solo daha uzak istasyona ulaşma hedefini ekler. Otomatik ateş karar yükünü tamamen ortadan kaldırmamalı; yalnız kalabalık ve canı artırmak içerik derinliği değildir.

Model üretimini büyütmeden incelenecek riskler: boş bekleme ve uzun geçiş; ani/haksız ölüm; vagon şansının oyuncu kararını ezmesi; silah/turret/dron tercihlerinin hep aynı olması; Battle sonucunun anlaşılmaması; Solo'da yeniden deneme/ilerleme amacının belirsizliği. Küçük dış test ilk anlaşılabilirlik/keyif sorunlarını bulur; istatistiksel retention veya gelir kanıtı değildir. Oyuncu paylaşımını kullanıcı yapar/onaylar; bu plan dış kişilere otomatik mesaj yetkisi vermez.

Ürün ölçümleri iki modda ayrı: eğitim/ilk istasyon tamamlama, ölüm/terk nedeni, yeniden koşu, istasyon/koşu süresi, D1/D7 geri dönüş. Gelir bağlandığında reklam teklif/kabul/tamamlama ve reklam sonrası terk, IAP ve oyuncu başına net gelir eklenir. Telemetri sağlayıcısı/kimlik/izin tasarımı seçilmeden harici SDK veya veri aktarımı eklenmez.

Reklam videosunun indirtmesi ile oyuncunun kalıp gelir oluşturması ayrı doğrulamalardır. Aynı cohort için seçilmiş zaman penceresindeki net oyuncu geliri, edinme maliyeti ve diğer değişken giderleri karşılamalıdır. Brüt reklam harcaması getirisi (ROAS) %100 olsa bile komisyon, iade, vergi ve diğer giderlerden sonra kâr garanti değildir. Sektör geneli tek bir D1/CPI/eCPM sayısı bu oyun için başarı eşiği kabul edilmez. Büyük reklam harcaması önce yapılmaz; onaylı küçük deney, yeterli gözlem süresi ve önceden seçilen durdurma kuralı kullanılır.

## Güncel birincil kaynaklar — 2026-10-09

- [Google Play: öne çıkarılma](https://google.play/business/guides/featuring/): oyun ve tanıtım içeriği kalitesi birlikte değerlendirilir; yarar/keyif, deneyim, teknik kalite ve gizlilik/güvenlik alanları vardır. Bunlara uyum otomatik tanıtım garantisi değildir.
- [Android: temel kullanıcı değeri](https://developer.android.com/quality/core-value): ilk kullanım ve zaman içinde keyif; kullanıcı etkileşimi/aktif kullanıcı ve kaldırma gibi ölçüler. İndirme tek başına oyun değerinin kanıtı değildir.
- [Android vitals: yavaş oyun oturumları](https://developer.android.com/google/play/vitals/slow-session): 20/30 FPS oturum ölçüleri; teknik kabulümüz yalnız ortalama FPS değildir. Bunlar bizim A54 için 60 FPS hedefimizden ayrı Play ölçütleridir.
- [Unity 6: mobil post-processing](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/integration-with-post-processing.html): efektler kare bütçesi tüketebilir; düşük cihaz için Gaussian alan derinliği önerisi vardır. Bu kaynak URP içindir; mevcut Built-in projede kurulu özellik gibi uygulanmaz.
- [Unity: ROAS kampanyaları](https://docs.unity.com/en-us/grow/acquire/campaigns/roas/intro-to-roas-campaigns): edinme maliyeti/gelir ve ölçüm pencereleri; kampanya hedefi sonuç garantisi değildir. Sağlayıcı seçimi veya harcama onayı sayılmaz.
