# Oyun tasarımı

Durum: kullanıcının 2026-10-07 tarihinde anlattığı hedefler ve açıkça işaretlenen öneriler. Sayısal değerler ilk denge denemeleri içindir.

## Oyun kimliği

Otomasyonla istasyonlar arasında hareket eden bir trende zombi salgını sırasında hayatta kalma. Savunma, rastgele vagon ataması ve koşu içindeki ekipman gelişimi birleşir. İlk hedef mümkün olduğunca çok istasyondan sağ çıkmaktır. Geniş roguelike yetenek/ödül sistemi sonraki kapsamdır.

Her vagon ayrı bir kimliğe, kapılara ve savunma kapasitesine sahiptir. Kapıların maksimum canı vagonun güvenliğini belirler. Şans, her yolculukta oyuncunun hangi vagona atanacağını belirler.

İlk prototip `Assets/Scenes/SampleScene 2.unity` yerleşim referansıdır: uzun dikdörtgen vagonlar, oyuncuyu izleyen kamera ve trenin iki yanından saldırılar. Güncel D23 görselleri fiziksel oranları ve 4–6 kapılı vagon hedefini belirler; ölçüler VISUAL_SCALE.md ve MODEL_CONTRACT.md içindedir. Kare/tek kapılı/tek taraflı test sahneleri bu hedefin yerine geçmez. 2026-10-08 karşılaştırması ve gerçek geometriye geçiş kabulü `VISION_AND_INTEGRATION.md` içinde kayıtlıdır.

Kararlaştırıldı: oynanış zemini sabit kalır, çevre/istasyonun görsel hareketi tren yolculuğunu oluşturur. İçeri girmiş zombiler yolculukta trende kalır; dışarıdakiler istasyonda bırakılır.

## Mod hedefleri — 2026-10-07 ek vizyon

Ortak tren/kapı/zombi/silah/koşu temeli iki moda hizmet edecek. Kullanıcı ilk varsayılan hedef olarak **Battle** düşündüğünü belirtti: nickli botlar komşu vagonlarda oynar, oyuncu hayatta kalanlar arasında birinci olmaya çalışır. En az üç bot beceri profili (başlangıç/orta/iyi düzeyi) hedeflenir. “Trene bin” giriş sunumu, yükleme ve yarış atmosferi daha sonraki arayüz aşamasıdır. Bu yerel bot hedefi için gerçek multiplayer, matchmaking veya ağ sunucusu henüz talep edilmiş değildir.

**Tek oyunculu ilerleme/hikâye:** rakip bot yoktur; rastgele vagon, kalıcı koşu ekipmanı ve daha ileri istasyonlara ulaşma odağı korunur. TrainIntegration artık Battle/Solo seçimi, dört bot ve üç beceri profili içerir; tamamlanmış hikâye sunumu değildir. Botlar ortak ateş/onarım/ekonomi çekirdeğini kullanır. D33: insan ölünce sırası gösterilip koşu biter; yalnız insan hayattaysa birincilikle biter; aynı karede elenenler aynı sırayı paylaşır. Yaşayan katılımcılar ayrı vagonlara atanır, elenen bot geri doğmaz. 5 vagon mevcut temel, 10 vagon olası genişlemedir. Ayrıntı [BOTS.md](BOTS.md).

## İstasyon ve yolculuk döngüsü

Kararlaştırıldı: tren istasyonda süreye bağlı bekler; ilk savunma 60 saniyedir, tüm zombilerin ölmesi kalkış için zorunlu değildir. Süreler veriden ayarlanır. Bu, otomasyon hikayesiyle uyumludur ve kalan tek bir düşmanın döngüyü kilitlemesini önler. İleride farklı istasyonlar için ayrı bitiş kuralları eklenebilir.

| Evre | İlk değer / davranış | Sorumluluk |
| --- | --- | --- |
| İlk yaklaşma | Yaklaşık 10 saniye; yumuşak yavaşlama | Oyuncu hazırlık yapabilir; ilk istasyon kurulur. |
| Savunma | Kararlaştırıldı: ilk istasyonda 60 saniye | Spawn programı işler; kapılara ve içeri giren zombilere karşı savaşılır. |
| Kalkış uyarısı | Yaklaşık 3 saniye | Yeni üretim durur; oyuncuya kalkış bildirilir. |
| Kalkış | Yaklaşık 3 saniyelik görünür hızlanma | İstasyon bağlantıları güvenle kesilir. |
| Geçiş | Kararma; karanlıkta 10 saniyelik sayaç | Trenin sesi sürer; bir kez rastgele vagon ataması yapılır. |
| Yeni yaklaşma | Görüntü açılır; yaklaşık 3 saniye sonra yavaşlama başlar | Sonraki istasyon hazır edilir; oyuncu hazırlık yapabilir. |
| Yeni savunma | Tren tamamen durunca | Kurulum hazırsa yeni spawn programı başlar. |

Fade süresi ve yavaşlama süresi ayrı ayarlardır; 10 saniyelik karanlık sayaca kendiliğinden eklenmiş kabul edilmez. Kalkış uyarısının 60 saniyeye dahil olup olmayacağı ilk denge denemesinde netleştirilecek. İlk öneri: 60 saniye savunma + 3 saniye uyarı.

Oyuncu ölümü her evreden `GameOver` durumuna geçebilir. Gerçek pause; hareketi, AI'ı, spawnı, hasarı ve onarım zamanını birlikte durdurur. Geçiş görüntüsü/sesi gerekiyorsa ayrı zaman kaynağı kullanır.

Kararlaştırıldı: karanlık geçişte oynanış durur; görünür yolculukta savaş devam eder. Fade başından görüntü/kontrol geri gelene kadar AI, atış, hasar, spawn ve onarım askıdadır; trenin geçiş sunumu/sesi sürer. Bu süre için saldırı/spawn birikimi oluşmaz.

## Kapılar ve onarım

D23: aynı gövde ölçüsünde vagon başına 4–6 kapı bulunabilir; her iki yanda dış çiftler korunur, bir veya iki orta kapı sağlam duvarla değişebilir. Kapı sayısı, kapı canından ayrı şans/zorluk ayarıdır. Az kapı daha az giriş cephesi sağlar; kendiliğinden güvenlik garantisi değildir. Kamera/ölçek ve yalnız referans olarak alınan görseller VISUAL_SCALE.md içindedir.

- Sağlam kapı zombi yürüyüşünü engeller, **üst camından dışarıya ateş edilebilir**. D24 ile iki taraftaki yan pencere camlarından da ateş edilir; metal dikmeler, çerçeveler, kapı alt paneli ve duvarın dolu metalleri mermiyi engeller. Pencereler beden geçişi değildir. Oyuncu ve kapı önüne kurulan turretler zombileri kapıyı kırmadan vurabilir; kapı dayanıklılığı bu savunma için zaman kazandırır (D15/D24).
- Oyuncu ilk kapsamda kapı sağlam/kırık/onarılmış olsa da atandığı vagondan dışarı yürüyemez. Bu, düşman geçişi ve atıştan ayrı bir hareket iznidir. İleride istasyona inilen görevlerde izin açıkça değiştirilebilir (D16).
- Örnek: 10 canlık kapıyı 8 hasarlık güçlü zombi iki vuruşta, 2 hasarlık zayıf zombi beş vuruşta kırar.
- Can sıfıra düşünce kapı `Broken` olur, geçiş açılır. Kapı nesnesi/kalıcı kapı kimliği korunur; kırık kapı yeniden kurulunca hasar çekirdeğinde yeni dayanıklılık yaşamı başlar.
- Oyuncu hasarlı sağlam veya kırık kapının yakınına gelip yaklaşık 3 saniye alanda kalırsa onarım tamamlanır; slider ilerlemeyi gösterir. Sağlam kapı bakımında dayanıklılık yaşamı korunur.
- Uzaklaşma, ölüm veya vagon değişimi onarımı iptal eder. Yarım onarım kapıya can vermez; yeniden denemede sayaç sıfırdan başlar.
- Öneri: tamir sırasında ateş edilebilir; tetik alanında olmak gerekir, ayrıca tuşa basmak gerekmez.
- Geçitte bir karakter varken kapı onun içine kapatılmaz. Onarım hazır olduğunda yeni girişler durdurulur, geçitte olanlar güvenle tamamlar; fiziksel kapanış alan boşaldığında uygulanır. Sıkışma politikası ilk testte doğrulanır.
- D20: başarılı onarım maksimum canı doldurur. Varsayılan oyuncu hasarı onarımı kesmez; kapı hasarı ilerlemeyi sıfırlar. İki kesilme kuralı ayrı ayarlanabilir. Hasar yine uygulanır; ölüm onarımı iptal eder. Savaşın/dalganın bitmesi şart değildir.
- Ücretsiz yakınlık onarımının bedeli oyuncunun zamanı ve konumudur. Anlık ücretli tamir aynı yerel onarım kurallarını kullanır; tıkanmış geçitte collider zorla kapatılmaz.

## Zombiler

Ortak döngü: doğuş → atanmış vagona yaklaşma → kapı seçimi → kapıya saldırma / açık geçitten girme → erişilebilir oyuncu veya savunmaya saldırma → ölüm → havuza dönüş.

NavMesh yol bulur; kapıya saldırma veya oyuncuyu hedefleme kararı zombi davranışına aittir. Türler hız, can, saldırı hasarı, saldırı aralığı, ödül ve hedefleme profiliyle ayrılır. İlk tür normal yürüyen zombidir; hızlı ve dayanıklı türler ortak davranış doğrulandıktan sonra eklenir.

Her zombi doğarken bir `WagonId` alır. Sırf oyuncu yer değiştirdi diye tüm zombiler onun vagonuna akmaz. Atanmış vagona alternatif kapıdan ulaşma mümkündür; vagonlar arası dolaşma ayrı tasarım kararıdır. Ulaşılamayan hedef öldürülmüş sayılmaz; yeniden değerlendirilir veya istasyon sonu kurallarıyla temizlenir.

Kararlaştırıldı: kalkış sınırında içerideki zombiler trende kalır. Dışarıdakiler istasyonda bırakılır ve görünmez olduklarında ödülsüz havuza döner. Geçitteki zombi için içeride/dışarıda olma durumu geçiş tamamlanmasıyla kesinleştirilir; ayrıntılı teknik kural `ARCHITECTURE.md` içindedir.

## Boş vagon ve güvenli atama

2026-10-08 kullanıcı geri bildirimi: bir vagonda yaşayan insan/bot savunmacı yoksa o vagona **yeni zombi üretilmez**. Önceki saldırıdan kalan içerideki zombiler ve kapı hasarı korunur; sonraki istasyonda o vagona yaşayan biri atanırsa yeni saldırı tekrar başlayabilir. Savunmacı öldüğü anda yeni üretim uygunluğu kapanır; var olan dış/iç zombiler anında silinmez ve normal kalkış kuralını izler. Boş vagonun birikmiş spawn bütçesi sonraki atamada topluca boşaltılmaz. Turretin tek başına vagonu saldırı hedefi saydırması seçilmedi; şimdiki uygunluk insan/bot canına aittir.

Kullanıcıyla seçildi: vagon atamasında bedenle çakışmayan yazılmış adaylar arasında en yakın zombiye uzaklığı en yüksek nokta seçilir; ardından varsayılan **2 saniye**, Inspector'dan ayarlanabilir hasar koruması verilir. Süre karanlıkta tükenmez; görünür oynanışta ateş/onarım sürer. Bu, çok dolu bir vagonun her zaman kazanılabilir olacağını garanti etmez; vagon riski korunur. Hiç fiziksel boş aday bulunamaması ayrı, açık tanılı bir durumdur.

## Battle botları ve uzak vagonlar

Mod/Bot ilk sürümünde beş vagonlu Battle, insan ve dört yerel botla oynanır; üç beceri profili ortak hareket/ateş/onarım/ekonomi kurallarını kullanır. Elenen bot aynı koşuda geri doğmaz. Rastgele atama yaşayan katılımcıları ayrı vagonlara dağıtır; boş vagon olabilir. D33: insan ölünce sırasıyla koşu biter, yalnız insan kalınca birincilik; aynı karede elenenler ortak sıra. Botsuz ilerleme modu ayrı seçilir. Bu ilk sürüm gerçek multiplayer veya tamamlanmış hikâye içeriği değildir; ayrıntı BOTS.md.

Görünürlük, savunmacı varlığı ve simülasyon üç ayrı kavramdır. Oyuncu vagonu ve görünen komşulardaki aksiyon sunulur. Uzak vagonların modeli/VFX/ses maliyeti azaltılabilir; kapı/mermi/can/öldürme/ödül durumu görünmediği için sıfırlanamaz. Önce ortak bot davranışı doğrulanacak, sonra ölçümle uzak simülasyon seviyesi geliştirilecek. Yaklaşık savaş hesabı yakın savaşla farklı sonuç üretebilir; karşılaştırma testi olmadan eşdeğer sayılmaz. Uzak → görünür geçişte yeni zombi/ölüm/ödül yaratmadan kayıtlı durum gösterilmelidir. Bu optimizasyon henüz uygulanmadı.

## Can, silah ve hasar

Oyuncu, kapı, zombi ve gerektiğinde turret aynı hasar sözleşmesini kullanır. Maksimum can ile mevcut can ayrıdır. Yelek gibi zırh, bağışıklık veya maksimum can artışı sonradan aynı sözleşmeye eklenen etkilerdir; ilk aşamada hepsi uygulanmaz.

Başlangıç silahı tabanca; satın alınabilecekler hafif makineli, tüfek ve pompalı. Silah türü, ateş aralığı, menzil, hasar, saçılma/pellet sayısı, mermi ve efekt ayarları veridir. Oyuncu/turret/dron aynı silah çekirdeğini farklı hedefleme ve hareketle kullanır.

Kararlaştırıldı (D13/D14): sol hareket joystick'i, otomatik nişan/ateş; ilk tabancada sınırlı şarjör, sınırsız yedek mermi, otomatik doldurma. M3'te hareket–ateş–onarım deneyimi değerlendirilir; kullanıcı tercihiyle rutin kontroller PC'de, uygun zamanda cihazda yapılır. Tam silah çeşitliliği daha sonra eklenecek.

Satın alınmış Hovl projectile assetleri görsel sunumda korunur. Gerçek vuruş tespiti efektlerden bağımsızdır. İlk öneri tabanca/tüfek/turret için raycast tabanlı anlık isabet + görsel mermi; yavaş roket vb. için ileride kontrollü fiziksel projectile. Pompalıda saçmalar kasıtlı ayrı isabetlerdir.

## Turret ve dron

- D31: turret satın alma anındaki oyuncu konumuna kurulur; uygun zemin/engel ve vagon kotası kontrol edilir. Sabit kurulum noktası aranmaz.
- Normal ve gelişmiş turret: 360 derece dönebilir; menzilinde, görüşü açık en yakın düşmanı hedefler. Sağlam kapı camından ve yan pencere açıklığından ateş edebilir; metal dikme/çerçeve veya dolu panel arkasındaki hedefi vuramaz. Kapı önüne kurulum temel savunma taktiğidir; aynı atış geometrisi oyuncu silahlarıyla paylaşılır. Turretin gerçek namlu yüksekliği kendi aşamasında açıklıklarla sınanır.
- Her ürünün ateş aralığı, hasarı ve mermi kapasitesi ayrı veridir. Örnek kapasite 50, örnek aralık 1 saniye; kesin denge değeri değildir.
- D29: her turret türünden vagon başına en fazla 2 adet; oyuncuyla taşınmaz, kurulduğu vagonda kalır.
- Mermisi biten turret bir kez kapanır, patlama efekti oynatır ve slotu boşaltır. D29: bitiş patlaması yakındaki zombilere bir kez alan hasarı verir.
- Oyuncuda en fazla 1 dron; oyuncuyu yumuşak takip ve küçük uçuş salınımlarıyla izler. Yakındaki erişilebilir/görüşü açık düşmana ateş eder.
- Dronun mermisi bitince kamikaze durumu başlar. Hedef varsa ona giderek bir kez alan hasarı verir; hedef yoksa sınırlı süre bekler ve güvenle kapanır. Hedefin arada ölmesi, oyuncunun ölmesi ve vagon değişimi tanımlı durumlar olmalıdır.
- D32: kamikaze erişebileceği en yakın zombiye gider; kapalı kapı/cam/metal uçuşu engeller. Süre içinde hedef bulunamaz/ulaşılamazsa hasarsız kapanır; metal patlamayı da keser. Süre/hız/hasar ayarlanabilir; ayrıntılar DRONE.md.

## Para ve mağaza

Zombi ölümü bir kez ödül verir; havuza geri verme veya istasyon temizliği ödül değildir. İlk öneri: ödül öldüren oyuncuya ya da ona ait turret/drona aittir; otomatik cüzdana eklenir. Fiziksel para toplama daha sonra aynı ödül kaynağına bağlanabilir.

Ürünler: iyileştirme, mevcut vagondaki kapıları anlık onarma, silahlar, turretler, dron; sonradan maksimum can/zırh geliştirmeleri.

Kararlaştırılan iyileştirme: `min(eksikCan, max(30, mevcutCan * 0.5))`. Örnekler: 10/100 → 40/100, 50/100 → 80/100, 80/100 → 100/100. Bu kural mevcut can arttıkça daha fazla iyileştirir; kullanıcı bu davranışı seçti. İyileştirme maksimumu aşmaz, tam canda işlem para harcamaz. Kesirli değerlerin UI'da yuvarlanması uygulamada açıkça belirlenir; hesaplanan can kaybolmaz.

Satın alma yalnızca uygulanabilir ürün için para düşer. Yetersiz para, dolu slot, dron sınırı, tam can veya kurulum hatasında ücret/ürün kaybı olmaz. Oyuncu vagon değiştirirken açık mağazanın işlemleri yeni bağlamı kontrol eder.

Kararlaştırıldı (D25): mağaza savaşta ve görünür yolculukta kullanılabilir, oyun devam eder. Karanlık geçişte/ölümde alım kapanır. D26: ilk silah alımı açıp doldurur; tekrar alım eksik mermiyi aynı fiyatla tamamlar, doluyken para harcanmaz. M7a ilk uygulamada ödülü otomatik cüzdana ekler; fiziksel pickup uygulanmadı. Ayarlanabilir fiyat/ödüller, bağlama ve PC kanıtı [ECONOMY.md](ECONOMY.md) içinde.

Kullanıcının Battle sonuç 3×, hikâye devam/3×, reklam kaldırma ve yardım teklifi fikirleri alındı. Ekonomi etkisi, kalıcı harcamalar ve teslim sınırları açık; kaynaklı alternatifler MONETIZATION.md içindedir. M7a yalnız koşu cüzdanı/mağazadır; servis entegrasyonu M9 aşamasındadır.

## Etap verisi ve kalıcılık

Bir istasyon verisi: süre, yerleşim, vagon başına spawn grupları, türler, başlangıç gecikmeleri, üretim aralıkları, toplam bütçe, canlı düşman sınırı ve ödül ayarları.

Zorluk yalnızca her istasyonda daha çok düşman üretmek değildir: aynı canlı düşman sınırı içinde tür dağılımı, tempo ve kapı baskısı ayarlanabilir. Güvenli ilk istasyon, giderek artan baskı ve periyodik nefes alma istasyonları önerilir.

Kararlaştırıldı: oyuncunun canı, parası, silahı ve dronu onunla gider. Kapı hasarı ve turretler kurulu oldukları vagonda kalır. İçeri girmiş zombiler de vagon durumuna dahildir. Mimari oyuncu ve vagon durumlarını ayrı saklar; istasyon değişimi koşuyu sıfırlamaz. Koşu içindeki kalıcılık ile uygulama kapanıp açılınca devam etme farklı özelliklerdir; disk kayıt M8 kapsamıdır.

Koşular arası kalıcı kazanımlar açık kalır; uygulama kapanınca devam için ilk hedef aşağıda kayıtlıdır. İlk istasyon döngüsü iki vagonla kanıtlanıp sonra beş vagona genişletilecek; bu geliştirme sırası hedef tren kapsamını küçültmez.

Kayıt için ilk kullanıcı seçimi mevcut koşuya devam, ölümde yeni koşudur; kalıcı geliştirmeler henüz seçilmedi. Kullanıcı ardından Google Play ile kayıt sağlayıcısını değerlendirmek istediğini ve ayrıntıda emin olmadığını belirtti. Sağlayıcı kesinleşmedi. [Google Play Games Saved Games](https://developer.android.com/games/pgs/savedgames) ilerleme verisini Google sunucularına kaydetme/geri alma ve bağlantı geldikten sonra senkronlama sunar; oyunun kendi durum verisini yazıp okuması ve yerel/bulut çakışmasını yönetmesi gerekir. Oturum açmak tek başına bizim koşu durumumuzu otomatik kaydetmez. Uygulama M8'de, Google adaptörü doğrulaması/kurulumu uygun servis aşamasında yapılır; M4 runtime Google SDK'sına bağlanmaz.

Mevcut atama: tüm vagonlar eşit olasılıklı, aynı vagon tekrar çıkabilir. Doğuş noktası duvar içinde veya karakterle çakışacak şekilde seçilmez. D28 ile geçerli adaylarda en yüksek düşman mesafesi ve ayarlanabilir 2 saniye hasar koruması kullanılır. Battle ataması RunMatch tarafından tek canlı savunucu/vagon kuralıyla birlikte koordine edilir; insan ataması bot kaydını ezmez. Solo aynı insan/vagon çekirdeğiyle botsuz çalışır.
