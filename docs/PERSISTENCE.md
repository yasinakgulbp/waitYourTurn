# Yerel koşu kaydı — M8a

**D39/D40 / M8s:** şema v3, açılmış vagon sınırı, ara kapı durumları ve alınmış cephane sandıklarını içerir. Bağlantı koridorundaki Solo konumu ve doğduğu havuzdan başka vagona yürümüş içerideki zombi konumu doğrulanır. Kapı topolojisi NavMesh yerleştirmesinden önce yüklenir. Önceki v1/v2 dosyası açıkça reddedilir; otomatik dönüştürme yapılmaz. Kalıcı profil koşudan ayrı dosyada tutulur. [Tasarım](PROGRESSION_DESIGN.md).

Kayıt hedefi hayattayken ara verilip aynı koşuya devam etmektir; ölümden sonra Solo/Battle geri açılmaz. Solo yeni challenge ilk istasyon/kuyruktan başlar. Google Play bulut, giriş, reklam ödülü ve IAP teslimi bu aşamada bağlanmaz. Cihaz kaydı sağlayıcısı `LocalRunStore`, sahne bağlayıcısı `RunPersistence`, veri sözleşmesi `RunSnapshot` v3 olur.

## Kullanım ve yaşam döngüsü

TrainIntegration sahnesinde kayıt kökü kurulu olur; yeni Integration builder da kurar. Ayrı kurulum menüsü `Wait Your Turn/Save/Install Local Save` geometri/NavMesh üretmez. Editor ilk Play'de yeni koşu açar: **F5 kaydet, F6 devam et, F4 kabul kontrolü**. `resumeInEditor` açılırsa Editor da otomatik kayıt/devam kullanır. Bu ayrım laboratuvarların eski bir savaştan başlamasını önler; mobil build otomatik devam eder.

Build: açılışta uygun kayıt varsa yüklenir; yoksa normal yeni koşu çalışır. Varsayılan beş saniyede bir, odağı kaybedince, uygulama arka plana alınınca ve normal kapanışta kayıt alınır. İşletim sisteminin aniden süreci öldürmesinde kapanış callback'i garanti değildir; son başarılı periyodik kayıt esas alınır. `autosaveSeconds` Inspector ayarıdır. Oyun kapalıyken istasyon/savaş süresi ilerlemez.

Dosyalar `Application.persistentDataPath/active-run.json` altındadır. F4 tamamen ayrı `acceptance-only/run.json` kullanır. Test tuşları geliştirici sahnesi içindir; son oyuncu menüsü değildir.

## Kaydedilen sahiplik

| Sahip | Veri |
| --- | --- |
| Koşu | Mod, koşu kimliği/tohum, istasyon/evre/kalan süreyi belirleyen elapsed, atama sayısı, insan ve bot atama RNG durumları |
| İnsan/bot | Ayrı can/maksimum can, konum/yön, koruma süresi, para, sahip olunan silahlar, seçili silah, şarjör/yedek, doldurma/ateş/satın alma kalan süreleri, onarım hedefi/ilerlemesi |
| Battle | Yaşayan katılımcılar ve eleme sıraları; elenmiş bot geri doğmaz |
| Vagon | Kapı canı, açık/kapanmayı bekleyen geçit; canlı zombinin profili/canı/konumu/giriş kapısı/doğduğu istasyon/trende kalma bilgisi ve saldırı beklemesi |
| Savunma | Turret tipi/konumu/slotu/mermisi ve katılımcı sahibi; dronun mermisi, konumu ve sonlu kamikaze arama süresi |
| Spawn | Akış başına üretilen miktar/başarısız deneme/bekleme, sınırlı istek kuyruğu ve sıradaki spawn anchor sayacı |

Runtime instance ID/LifeVersion dosyaya taşınmaz. Yüklenen beden yeni runtime yaşamı alır; Health restore `Died`/öldürme ödülü üretmez. Ödül gözlemcileri yeni sahip kimliğine bağlanır. Satın alma bağlamı yenilenir; eski UI isteği yeni koşuya ürün teslim edemez. Statik tanımlar canlı veriyle değiştirilmez.

İçerik anahtarı her Inspector alanının otomatik migration sistemi değildir. Kayıt sözleşmesini etkileyen geometri/kural değişiminde `contentRevision` yükseltilir. Mevcut anahtar silah/yerleşim/profil değişikliklerini ayrıca yakalar.

Unity inline sınıflardaki null alanları JSON'da boş nesneye çevirebildiği için dron varlığı `hasDrone` ile açıkça kaydedilir; boş nesne satın alınmış dron sayılmaz. F4 dronlu Battle ve dronsuz Solo için gerçek dosyadan geri yüklemeyi ayrı kontrol eder.

## Yükleme sırası ve güvenlik

Önce şema/içerik, evre/süre, katılımcı kimliği/sıra/tek-vagon sahipliği, can/mermi sınırları, kapı/geçit, profil, vagon kapasitesi, turret kotası/sahibi ve spawn kuyruğu doğrulanır. Uygun olmayan kayıt mevcut oyuna uygulanmaz. İçerik anahtarı yerleşim, silah, bot ve spawn tanımlarıyla ayrıca sürüm anahtarını kapsar; henüz migration yoktur. Uyumsuz veri yeni koşuyla değiştirilir.

Yüklerken oynanış kapatılır; mevcut aktörler temizlenir, NavMesh geçitlerinin güncellenmesine iki kare ayrılır. Evre doğrudan geri kurulur; normal evre callback'i yeniden tetiklenmez. Katılımcılar/sahiplik, kapılar, zombiler, savunmalar ve üretim sırası kurulur; onarım en son yeni yaşam/revizyonlara bağlanır. Fizik yerleşimi yine de başarısız olursa yarım sahneyle devam edilmez; yeni koşuya dönülür.

Yüklemede zombi yolu/nişan hedefi yeniden hesaplanır; başlamış melee windup yeniden oynatılmaz, kalan saldırı beklemesi korunur. Dalıştaki dron kalan sonlu süreyle tekrar hedef arar. Biten patlamaların kozmetik kuyruğu ve tren traverslerinin görsel fazı kayıt konusu değildir; hasar/ödül yeniden uygulanmaz. Bu kayıt kare kare deterministik savaş tekrarı değildir; canlı oyun durumundan devam eder.

Dosya boyutu 2 MiB ile sınırlı; checksum bozulmayı tespit eder, hile koruması değildir. Yazma geçici dosya/flush/atomik değiştirme ve bir önceki yedekle yapılır. Bozuk ana dosyada yedek denenir; tamamlanmış/yeni koşuda tombstone ile eski canlı kayıt ve yedek emekliye ayrılır. Önce kalıcı emekli koşu kimliği yazılır; yarım invalidation eski koşuyu yedekten diriltmemelidir. Disk hatası oyunu çökertmez, kayıt durumu başarısızlık olarak kalır.

## Doğrulama ve açık işler

2026-10-09 PC: 38 Run + 25 silah + 41 can/onarım testi PASS. F4 gerçek dosyadan dronlu Battle/dronsuz Solo, yedi evre, eleme/RNG/mermi/onarım kalıcılığı, geçersiz kayıt reddi, ölüm tombstone ve yön davranışını doğruladı. F7 bot, F9 mağaza ve F10 tren kontrolleri PASS. Kanıt [generated/m8a-pc-acceptance.txt](generated/m8a-pc-acceptance.txt). Yeni bir Play oturumunda F6 ile gerçek dosyadan devam da geçti; Android kabulü değildir.

Bu aşama yerel kayıt temelidir. Android arka plan/süreç kapatma/yeniden açma ve dosya değiştirme desteği cihazda ayrıca doğrulanmalıdır. Bulut çakışma çözümü, kullanıcıya dönük devam menüsü/öğretici, uzun oturum dengesi ve uzak vagon optimizasyonu açık kalır. M8'in tamamı bitmiş sayılmaz.

2026-10-09 D40: v3 sandık alınma bayraklarını ekler; v2 kayıt kontrollü reddedilir, geliştirme sürümünde göç yoktur. `player-profile.json` ayrı kalıcı jeton/silah profilidir, koşu snapshot'ına geri sarılmaz. Ödül ve sonuç kimliği birlikte atomik yazılır. Solo ölümde eski koşu emekliye ayrılır; sonraki oyun ilk istasyon/kuyruktan başlar. Canlı koşu kaydı yalnız ara verip geri açma içindir. Profil/sandık sınırları PROGRESSION_DESIGN.md içinde.
