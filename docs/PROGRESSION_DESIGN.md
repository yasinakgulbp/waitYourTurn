# İki mod, kalıcı ilerleme ve ilk ürün kapsamı

2026-10-09. Kullanıcı kararları ile mühendislik önerileri ayrı tutulur. M8s fiziksel Solo geçişi ve cephane sandığı; M8e sonuç ödülü, ayrı kalıcı cüzdan, silah açılışı ve küçük can/menzil geliştirmeleri uygulanmıştır. Hız geliştirmesi ve üretim menüsü henüz yoktur. PC kabulü ve ertelenen cihaz kontrolü ayrı izlenir.

## Kararlaştırılan deneyim

| Konu | Kullanıcı kararı |
| --- | --- |
| İlk yayın | Battle ve Solo eşit önemli. İlk kısa öğretici Solo üzerinden; ardından Battle erişimi. Açılma koşulu henüz seçilmedi. |
| Solo ilerleme | En zayıf kuyruk vagonundan başla; koşu parasıyla sıradaki daha güçlü vagonu aç. Yolculukta rastgele atama yerine fiziksel geçiş. |
| Solo saldırı | Açılmış vagonlar arasında geçiş serbest. Yeni saldırılar oyuncunun bulunduğu vagona yönelir; içeri girmiş zombiler korunur. Eski zombiler sırf vagon değiştirilince silinmez. |
| Ara kapı | Satın alma kilidi kaldırır; yakında düğmeyle aç/kapat. Zombiler açık geçitten takip eder, kapalıyı geçemez ve ilk sürümde kıramaz. Dış savunma kapısından ayrı mekanik. |
| Yeni vagon | İlk parça: açılan her yeni vagonda tek cephane sandığı. Yakında otomatik alınır, yalnız sahip olunan silahların cephanesini tamamlar. Doluyken harcanmaz; silah açmaz. Diğer ganimetler sonra. |
| Solo ölüm / yeni koşu | **D40:** her yeni challenge ilk istasyonda, en zayıf kuyruk vagonunda başlar. Vagon kilitleri, sandıklar, koşu altını, silah envanteri, zombiler ve savunmalar sıfırlanır. Yaşarken uygulamaya ara verme kaydı aynı koşuya devam içindir; ölümden sonra otomatik devam değildir. |
| Battle | Mevcut ayrı vagon, rastgele atama, eleme ve sıralama korunur. Yarış için zorunlu toplam zaman sınırı yok. |
| Kalıcı güç | Kullanıcı, can/menzil/hız geliştirmelerinin **iki modda da** çalışmasını seçti. Battle değerlerini eşitleme önerisi seçilmedi. |
| Kalıcı silah açılışı | Tabanca başlangıçta açık; diğer üç silah ana menüde bir kez açılır. Açılmış silahı koşu sırasında ayrıca koşu parasıyla alma hedefi. |
| Para | İki toplam kaynak: koşu parası ve sonuçtan az kazanılabilen/IAP ile alınabilen kalıcı para. Ayrı üçüncü kalıcı cüzdan eklenmez. İsimler/fiyatlar seçilmedi. |
| Sonuç | Battle sırasına göre azalan kalıcı ödül; Solo ilerlemesine göre ödül. Battle 3×, Solo video devamı ardından nihai sonuçta 3×; reddedilirse kısa geçiş reklamı hedefi D30. |

## Açık mantık riskleri ve ilk öneriler

- **Gelişmiş vagon avantajdır, otomatik zorunluluk değildir.** Zorluğu yalnız kapı canına bağlarsak oyuncu sürekli bir sonraki vagon fiyatını ödemeye zorlanır. İlk denge, eski vagonda iyi oynayarak biraz daha dayanmayı ve daha iyi vagonda gerçek rahatlamayı birlikte sağlamalı.
- **Sınırlı menzil/hız geliştirmesi geometriyi bozmamalı.** En düşük menzil kapı/camdan yaklaşan düşmana kullanılabilir olmalı; en yüksek menzil sonraki bütün vagonları vurabilmemeli. Silah temel tanımı sabit; oyuncu/bot istatistikleri üstüne sınırlandırılmış çarpan uygulanır. Can tavanının artışı mevcut canı otomatik doldurmaz; doldurma kuralı ayrıca seçilir.
- **Parayla alınan Battle gücü yarış algısını etkiler.** Kullanıcının iki mod kararı korunur; az ve açık üst sınırlar, anlaşılır güç seviyesi ve ücretsiz kazanma yolu önerilir. Bot güç/ustalık aralıkları veriyle tanımlanır; oyuncu yükseltince bütün rakipleri gizlice aynı oranda güçlendiren lastik bant kullanılmaz. Gerçek PvP eklenirse ayrı eşleştirme/adalet tasarımı gerekir.
- **Vagon değiştirerek baskı veya ödül üretme açığı bırakmayız.** Solo istasyonunun toplam spawn bütçesi vagon girip çıkınca sıfırlanmaz; bekleyen isteklerin yönlendirilmesi oyuncu varlığını izler. Mevcut `Unoccupied` akış bastırması Solo'da körlemesine yeniden kullanılmaz: boşken bastırılmış vagon sonradan açıldığında programın tamamen susmaması gerekir.
- **Sonsuz ödül/kayıt tekrarı yok.** Vagon açılması, ganimet kimliği/toplanması, kalıcı satın alma ve sonuç teslimi kayıtlıdır. Yükleme yeni sandık, ikinci temel ödül veya ikinci 3× üretmez. Sonuç ödülü kalan koşu parasına eşit değildir; aksi hâlde para harcamak oyuncuyu cezalandırır.
- **Daha iyi vagonu açınca mevcut turretler kendiliğinden taşınmaz.** Önceki sahiplik korunur; dron oyuncuyla gider. D39 ile açık bağlantıda iç zombi takibi, satın alma sonrası manuel kapı seçildi. Kapıda can/onarım yoktur; iç geçitte beden veya dron varken kapanış reddedilir.
- Doğal nickler, farklı karar gecikmeleri, kısa dolaşma, onarım/alışveriş ve botların değişik ustalıkları hedefe uygundur. Sahte internet eşleşmesi/gerçek insan bulundu iddiası önerilmez: yerel rakip yarışını mod açıklamasında belirtip girişte “Tren hazırlanıyor / Yolcular yerleşiyor” kullanabiliriz. Uzun rastgele bekleme eklemeyiz. İsim/mesaj ve sunum henüz uygulanmadı.

## Ortak parçalar ve mod sınırları

Yeni büyük framework yerine mevcut bağlayıcının dar sorumlulukları ayrılır. İstasyon zamanını `RunFlow`, saldırı/can/silah/onarımı mevcut ortak bileşenler yürütür.

| Parça | Sorumluluk ve sınır |
| --- | --- |
| Mod politikası | Battle katılımcı ataması/eleme; Solo başlangıç, açılmış vagonlar, geçiş ve ilerleme. `RunDriver` FadeIn atamasını mod politikasına devreder; Solo'ya Battle ataması uygulanmaz. |
| Geçiş geometrisi | Vagon uçları ve sabit bağlantı zemini; dış kapılardan ayrıdır. Oyuncu izinli vagon/bağlantı hacimlerinin birleşiminde yürür; bütün tren etrafını açık bırakan büyük bir dikdörtgen sınır kullanılmaz. |
| Güncel vagon bağlamı | Geçiş eşiğinde onarım, mağaza, turret yerleşimi, hedef kapsamı, kamera ve spawn hedefi birlikte güncellenir. Bu güncelleme teleport/yeniden doğma değildir; her geçişte 2 sn koruma verilmez. |
| Meta profil | Kalıcı bakiye, açık silahlar, yükseltme seviyeleri ve satın alma hakları; koşu cüzdanından ayrı. Statik ScriptableObject tanımlarına oyuncunun ilerlemesi yazılmaz. |
| Koşu ödülü | Mod/ilerleme/sıra üzerinden kesinleşmiş tek sonuç; reklam/IAP SDK'sına bağımlı değildir. Ürün ve callback teslimi sonradan adaptörle bağlanır. |
| Görünmeyen vagon | İlk olarak ölç; sonra insan+komşular ayrıntılı, uzak botlar düşük sıklıkta simüle edilebilir. Mantıksal can/para/kapı/enemy/ödül durumu korunur. Gizlemek tek başına simülasyon maliyetini kaldırmaz. |

`RunSnapshot` v3 açılmış vagon sınırını, ara kapıları, bağlantıdaki insanı, başka vagona yürümüş havuz düşmanını ve alınan sandıkları kaydeder. v1/v2 kontrollü reddedilir; bu geliştirme sürümünde otomatik göç yoktur. `player-profile.json` kalıcı cüzdan/silah açılışlarını bağımsız tutar; koşu kaydı veya ölüm bu profili sıfırlamaz.

## M8s uygulama sınırı

İlk iki komşu vagon üzerinde kilit/açma/yürüme/takip/kayıt sınanır; aynı kurulum mevcut beşli trenin dört bağlantısında kullanılır. Geçiş fiyatları 200/300/400/500 koşu parasıdır, yayın dengesi değildir. Solo kuyrukta başlar; karanlık geçişte rastgele atama yapılmaz. Vagonun orta bağlantı eşiği geçilince onarım, mağaza, turret kurulumu, spawn ve kamera bağlamı güncellenir; can, para, silah ve konum sıfırlanmaz, geçiş koruması verilmez.

Spawn tüm vagonlar için ayrı Solo bütçe üretmez: `wagon=-1` mobil bantları istasyonda tek program olur, her yeni istek güncel vagona yönelir. Sadece belirli vagona yazılan bantlar Battle içeriğidir; Solo programında en az bir mobil bant gerekir. Geçiş mevcut programı veya bekleyen istekleri sıfırlamaz. Zombi fiziksel olarak başka vagona geçse de doğduğu havuzda kalır; havuz kapasitesi, hasar ödülü ve kaydı aynı sahipliği kullanır.

Kullanıcı A54 geçiş testini sonraya bıraktı. Geçiş PC kanıtı `generated/solo-progression-pc-acceptance.txt` içinde; cihaz kabulü tamamlandı sayılmaz. Sandık ve minimum profil ayrıca eklenir; üretim ana menüsü, güç yükseltmesi ve reklam bu parçanın kapsamı değildir.

## Sandık ve minimum profil sözleşmesi

`SoloLoot` vagon başına alınma bayrağını yönetir; `HitscanWeapon.TryRefillOwned` mevcut silahları doldurur, seçili silahı ve ateş beklemesini değiştirmez. Sandık görselinin collider/NavMesh rolü yoktur. Yakınlık merkezinin yerel konumu (-3.6, 0.05, 0), yarıçapı 0.9 m; görsel merkez (-3.6, 0.3, 0), boyut 0.55 × 0.5 × 0.5 m. Mekanik konum ve yarıçap Inspector'dan ayarlanır; görsel model değişimi pickup kuralını değiştirmez.

`RunProfile` sonuç kesinleştikten sonra çalışır; `PlayerProfile` para/silah kuralları, `LocalProfileStore` atomik dosya teslimidir. Ödül ve koşu kimliği aynı yazmada kaydedilir; başarılı yazmadan canlı bakiye değişmez. Son 128 sonuç kimliği tutulur; normal akışta eski koşu kaydı ayrıca emekliye ayrılır. Bu yerel temel hile koruması veya gelecekteki reklam/IAP işlem makbuzu deposu değildir. Profil okunamıyorsa mevcut dosya korunur; sessizce boş profil yazılmaz. Eski acceptance fixture'ları gerçek profile para/harcama uygulamaz.

**Geçici denge:** Solo 2 + tamamlanan istasyon başına 3 jeton; Battle sıra ödülleri 20/12/8/3/2. SMG/tüfek/pompalı kalıcı açılışı 20/40/60 jeton. Değerler `RunProfile` Inspector alanlarıdır, yayın dengesi değildir. Tabanca ücretsiz açık; kalıcı açılış koşuya silah hediye etmez, koşu mağazasında ayrıca altınla alınır. Sonuç ekranında deneme profil paneli vardır; üretim ana menüsü sonraki adımdır. Ölüm sonrası otomatik diriltme yoktur; eski D30 reklamla canlandırma fikri ayrı karar/servis olmadan uygulanmaz. Reklam/IAP SDK henüz eklenmedi.

## D41 — Sınırlı kalıcı can ve menzil

İlk deneme her özellik için 5 seviyedir: seviye başına +%2, toplam en fazla +%10. `RunProfile.upgrades` içindeki `PermanentUpgradeRules` adımları 0–%2 arasında ve fiyatları ayarlanabilir tutar; sınır kodda 5 seviyedir. Deneme fiyatları 10/20/30/40/50 jeton. Satın alma yalnız kesinleşmiş koşu sonunda yapılır; seviye ve bakiye tek atomik profil yazmasında teslim edilir. Canlı koşuda satın alma, yetersiz para ve üst sınır reddedilir. Bu değerler ticari denge kabulü değildir.

Yeni Solo/Battle koşusunda yalnız insan oyuncuya uygulanır. Can `StartingMaximum × çarpan`, menzil her silahın sabit temel menzili × çarpan olur; tekrar başlatma veya yükleme üst üste bonus eklemez. Yeni koşu dolu canla başlar. Canlı kayıt yükleme mevcut canı doldurmaz; `PreserveCurrent` uygulanır. `ChangeMaxHealth` yaşam kimliğini değiştirmediğinden mevcut öldürme ödülü bağı korunur. Satın alma ölüyü diriltmez ve ölümde Solo'nun ilk istasyon/kuyruk kuralı değişmez. Bot, turret ve dron değerleri insan bonusunu almaz; ileride bot güç profilleri ayrıca tasarlanabilir.

`HitscanWeapon.Range` hem hedef seçimi/görüş hem gerçek ışın/pellet uzaklığında tek etkili menzildir. Mermi, damage, ateş/reload süreleri ve ortak `WeaponDefinition`/`WeaponSpec` değişmez. Cam/metal raycast kuralı aynı kalır; bonus metali geçirmez. Sabit fizik/model ölçüleri veya NavMesh yeniden üretilmez. Modifier yalnız oyuncu örneğine aittir; Combat bileşenleri meta profile bağımlı değildir.

`PlayerProfile` v2'dir. v1 para, silahlar ve sonuç makbuzları korunarak sıfır can/menzil seviyesiyle bellekte göç eder; sonraki başarılı profil yazması v2 kaydeder. `RunSnapshot` v3 kalır; insan bonusu kalıcı profilden yeniden uygulanır, ammo snapshot geri yüklenir. Hız, zırh ve geniş yetenek ağacı sonraki kapsamdır.

2026-10-09 14:50 Türkiye saati: genişletilmiş **tek kısa birleşik PC kontrolü PASS**. Önceki sandık/cüzdan/sonuç akışına v1 profil göçü, canlı satın alma reddi, fiyat/5-seviye sınırı, ölü oyuncuyu diriltmeme, yeni koşuda dolu artırılmış can/tek yaşam kimliği, dört silahın gerçek uzayan ışını ve metal engeli, hasarlı can/mermi kaydından iyileştirmesiz/katlanmasız devam ve iki modda uygulama eklendi. Kanıt `generated/permanent-upgrades-pc-acceptance.txt`. Eski uzun test paketleri yeniden çalıştırılmadı. A54 yeni güç/ganimet/profil kontrolü bekliyor; üretim menüsü, kullanıcı dengesi ve cihaz kabulü bu PC testinin yerine geçmez.

PC kısa birleşik kabul PASS (2026-10-09 11:33:21 UTC): tek sandık, dolu cephane, gerçek dosyadan ganimet/mermi devamı, Solo ölümde sıfır vagon/sandık/altın/koşu silahı, kalıcı bakiye korunması, iki modun tek sonuç ödülü, dosya tekrarında makbuz, yetersiz/tekrarlı kalıcı satın alma, iki modda silah açılışı ve ayrı koşu satın alması, disk hatası bildirimi. Kanıt `generated/loot-profile-pc-acceptance.txt`; test profili gerçek profilden ayrıdır. Menü: `Wait Your Turn/Progression/Install Loot and Profile` (Ctrl+Alt+F9), Play'de `Check Loot and Profile` (Ctrl+Alt+L). Önceki uzun kontroller bu turda tekrar çalıştırılmadı; A54 yeni sandık/profil kabulü bekliyor.

## İlk uygulanacak küçük parça

M8b mevcut Android temeli → M8s **iki vagon** Solo açma/geçiş/spawn/kayıt → M8e minimum sonuç ve kalıcı silah/tek sınırlı geliştirme taslağı → hafif kamera/ses ve oyuncu UI → kısa iki mod oyuncu denemesi. Bir vagon sanat örneği M8s geçit ölçüsü kabulünden sonra başlayabilir; bütün meta sisteminin bitmesi beklenmez.

M8s kabulü: Battle eski atama/eleme testleri geçer; Solo kuyrukta başlar, parası yetmeden vagon açamaz, bir kez ödeyip geri yürüyebilir; yeni spawn doğru vagona yönelir ve bütçe sıfırlanmaz; eski zombiler/kapı/turret korunur; bağlantıda kaydet/kapat/yükle çalışır; karakter dış kapıdan trenden ayrılamaz. Önce PC, ardından A54 kısa birleşik kontrol.

Bu parçada yeni hero koleksiyonu, günlük görev/enerji, sezon bileti, rastgele ücretli sandık, gerçek multiplayer ve uzak savaşın yeni simülatörü yok. İki modun ilk yayın kapsamı korunur; özellik yığınıyla yayın ertelenmez. Her parça sonunda çalışan çıktı/ölçüm ve devam kararı kaydedilir.

## Kaynaklardan alınan ders — 2026-10-09

| Birincil kaynak / veri sınırı | Bizde karşılığı |
| --- | --- |
| [GameAnalytics 2026 benchmarks](https://www.gameanalytics.com/reports/2026-mobile-pc-gaming-benchmarks): 2025 verisi, 16.000+ entegre mobil oyun, en az 1.000 MAU; medyan D1 yaklaşık %22, D7 <%4, oturum 3,1–3,5 dk. Tür ayrımı yok; bütün yayınların başarı oranı veya bizim hedefimiz değildir. | İlk kısa oturumda hareket→kapı savunması→onarım→kazanç→anlaşılır ilerleme hedefi. Kayıtla mola verilebilir; Battle zorla 3 dk'da bitirilmez. |
| [Bullet Echo açılış/geliştirme](https://zepto.helpshift.com/hc/en/10-bullet-echo/faq/1028-ways-of-unlocking-and-upgrading-heroes/): hero açma, eğitim, tier ve yetenek geliştirme belgelenmiş. Özel deney sonuçları/kâr açıklanmıyor. | Oynadıkça açılan yeni araç + küçük kalıcı gelişme. Kart/tier/batarya/abonelik yığını kopyalanmaz; bizim vagon ve silahlar anlaşılır karşılık verir. |
| [AppMagic Mobile Market Landscape 2026](https://appmagic.rocks/files/view/upload/Reports/EN_MobileMarkeLandscape2026.pdf), s.53–57: 2023–2025 tahmini mağaza indirme/IAP verisi; hypercasual'da hibrit ilerleme yönü. Bazı üst puzzle oyunlarında ilk üç teklif yaklaşık %40 gelir; bu teklif analizi ABD App Store örneğidir, reklam geliri/diğer mağazalar dahil değildir. | Az sayıda işe yarayan, açık ürünle başla. Puzzle fail-offer fiyatlarını tren oyununa “kanıtlanmış optimum fiyat” diye taşımayız. Görsel kanca yanında geri dönme amacı gerekir. |

Karşılaştırılacak kendi verimiz: ilk istasyon/öğretici tamamlama, ilk vagon/ürün alma, ölüm/çıkış nedeni, tekrar koşu ve D1/D7 (mod ayrı); gelir bağlanınca aynı cohort/ülke/kanalda edinme maliyeti ve net gelir. Önce kısa gerçek oynanış videosuyla oyunun kancası anlaşılır mı ve küçük oyuncu grubunda tekrar deneme isteği var mı incelenir. Bu ucuz inceleme büyük ücretli kampanyanın veya istatistiksel retention ölçümünün yerine geçmez; boşa büyük üretimi erken yakalar.
