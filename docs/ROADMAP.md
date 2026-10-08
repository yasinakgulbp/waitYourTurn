# Sıralı geliştirme planı

Durum: 2026-10-07 — M0 ve M2 tamamlandı. M3 tek vagon teknik parçası kuruldu; ilk sürüm PC'de 10/10 birleşik kontrolü geçti. D15/D16 cam kapı ve vagon sınırı revizyonu odaklı PC kontrolünü geçti (UTC 18:53:55); kullanıcı PC değerlendirmesini kabul etti; cihaz hissi ileride uygun zamanda kontrol edilecek. M1'in temel kapı/rota kontrolü tamamlandı; eski 30-agent gridinde gözlenen hedef yerleşimi sorunu büyük kalabalık için açık kalır. M3 gerçek brain sabit grid yerine oyuncu menzilini hedefler; bu büyük kalabalığın kanıtı değildir. Vagon kenarındaki yakın zeminler ortak NavMesh + carving kullanır. Önceki A54 performans/100-agent kanıtı eski link geometrisine aittir. Rutin testler PC'de; uygun zamanda önemli birleşik oynanış değişimi Android'de değerlendirilecek. Ayrıntılar NAVIGATION_LAB.md, COMBAT_LAB.md ve GAMEPLAY_LAB.md içinde.

## Öncelik mantığı

2026-10-08 ek durum: kullanıcı M4a PC değerlendirmesini kabul etti. M4b `TrainSandbox` beş vagonlu teknik parçası 10/10 geçişi geçti; kamera/oyuncu ataması, ayrı kapı durumları, iç zombi üyeliği ve terminal ölüm doğrulandı. 60 eşzamanlı zombilik kısa PC yük kontrolü geçti. Kullanıcı M4b teknik değerlendirmesini kabul etti; gerçek altı kapılı tren entegrasyonu ve botlar henüz yok. Ayrıntı RUN_LAB.md.

Önce navigasyon ve kapı geçişi teknik olarak kanıtlanır; aynı aşamada kaba cihaz/kalabalık ölçümü yapılır. Ortak hasar temeli ardından minimum mobil kontrol ve tabancayla tek vagonun döngüsü birleştirilir. Kullanıcı oynanış değerlendirmesi sonrası iki vagonla istasyon akışı kanıtlanır, sonra beş vagona genişletilir. Kullanıcının güncel tercihi rutin PC kontrolü, önemli birleşik değişimde uygun zamanda Android kontrolüdür. Silah çeşitliliği, geniş spawn sistemi ve savunmalar bunu izler.

İlk oynanabilir hedef: **bir vagon + bir kapı + bir zombi türü + minimum mobil kontrol + oyuncu tabancası; kapı kırılır, zombi girer, oyuncu hasar alır/vurur, kapı onarılır ve rota doğru değişir.** Bu tamamlanıp kullanıcı değerlendirmesi yapılmadan dron, çoklu silah ve mağaza yapılmaz; cihaz hissi kontrolü ayrıca korunur.

## Aşama tablosu

| Aşama | Çıktı | Bağımlılık | Durum |
| --- | --- | --- | --- |
| M0 | Korunan prototip ve geliştirme/test düzeni | Plan | Tamamlandı; referans/test sahnesi, AudioListener düzeltmesi ve Android lab buildi doğrulandı; hedef A54 |
| M1 | NavMesh/kapı geçişi ve kaba cihaz/kalabalık ölçümü | M0 | Temel rota/kapı 10/10; A54 ilk ölçümü kayıtlı. Yeni sürekli yüzeyde vagon içi kalabalık hedef yerleşimi M3/M6 için açık |
| M2 | Ortak can, hasar, ölüm | M1 deneme sonucu; kodu M1'e bağımlı değil | Tamamlandı; 14 test geçti, izole hitscan/can/ölüm Play kontrolü yapıldı |
| M3 | Mobil kontrollü tek vagon döngüsü ve oynanış değerlendirmesi | M1, M2 | GameplaySandbox ilk sürümünde birleşik mekanikler 10/10 PC PASS; D15/D16 revizyonunda cam/duvar/oyuncu sınırı ve gerçek hasar-giriş-onarım odaklı PC PASS. Kullanıcı PC oynanış değerlendirmesini kabul etti; cihaz hissi sonraki uygun kontrolde |
| M4a | İki vagonla istasyon/yolculuk/atama döngüsü | M3 değerlendirmesi | PC teknik kontrolü geçti: 10/10 geçiş ve 6 saf evre testi; kullanıcı değerlendirmesi kabul edildi. RUN_LAB.md |
| M4b | Kanıtlanan döngüyü beş vagona genişletme | M4a değerlendirmesi | Beş tek-kapılı lab vagonunda 10/10 geçiş + 60-zombi PC yük kontrolü PASS; kullanıcı teknik aşamayı kabul ederek M5'i istedi. Gerçek altı kapılı geometri/çok kapı hedef seçimi entegrasyonu ayrıca açık |
| M5 | Mobil kontrolü iyileştirme ve silah çeşitliliği | M2, M4b | PC teknik parça PASS: 13 silah testi, dört profil cam/duvar/alt panel kontrolü ve sınırlı mermili 10/10 vagon geçişi. D19 uygulanır; mevcut sol kontrol korunur. Kullanıcı/cihaz değerlendirmesi açık. WEAPONS_LAB.md |
| M6 | Veriyle yönetilen spawn, enemy çeşitleri ve kalabalık bütçesi | M3–M5 | Bekliyor |
| M7 | Para, mağaza, turret ve dron | M2, M4–M6 | Bekliyor |
| Mod/Bot | Battle için komşu vagon botları, en az 3 beceri profili ve eleme/sıralama; botsuz ilerleme modu politikası | M4b ortak döngü, M5/M6 savaş, M7 ekonomi | Tasarım hedefi kaydedildi; ortak mekanikler sağlamlaştıktan sonra ayrı aşama. Gerçek multiplayer veya matchmaking henüz kapsamda değil |
| M8 | Etap dengesi, kayıt ve koşu deneyimi | M4–M7 | Bekliyor |
| M9 | Mobil performans ve yayın hazırlığı | Önceki aşamalar | Bekliyor |

Performans ölçümü M1'den itibaren başlar; M9'a ertelenmez. Pooling M3'te temel yaşam döngüsü, M6'da geniş spawn sistemi olarak gelişir. Aşağıdaki maddelerde geçen M4, M4a/M4b bütününü ifade eder.

## Erken tasarım kararları ve çalışma sınırı

- Minimum mobil kontrol ve ilk tabanca kuralı M3 için seçildi: sol joystick + otomatik nişan/ateş; sınırlı şarjör + sınırsız yedek + otomatik doldurma (D13/D14). Tam silah çeşitliliği M5'te uygulanır.
- M4a öncesinde koşudan koşuya korunacak kazanımlar ve uygulama kapanınca devam beklentisi kâğıt üzerinde netleştirilir. Kayıt sistemi M8'de uygulanır.
- D17: ilk hedef mevcut koşuya devam ve ölümde yeni koşu; meta kazanımlar açık. Google Play Games Saved Games adaydır, kullanıcı sağlayıcıda emin değil; M8 kayıt tasarımı yerel/bulut adaptörünü gameplayden ayırır.
- D18: gelecekte varsayılan Battle/botlu yarış ile botsuz ilerleme modu; en az üç bot beceri profili. M4a laboratuvarı ortak döngüyü kanıtlar. Bot hareket/ateş/onarım/mağaza aynı kuralları kullanacak, rakip/sıralama politikası döngü motoruna gömülmeyecek.
- M7 öncesinde ödüllü reklam/IAP ürünleri, tekrar sınırları ve ekonomi etkisi kullanıcıyla tasarlanır. Entegrasyon M9'da kalır; kesinleşmemiş ödül/fiyat uydurulmaz.
- Her oturum tek, küçük bir kabul hedefiyle başlar. Teknik deneme sonuç vermiyorsa kapsam genişletilmez; bulgu ve sonraki deneme kaydedilir.
- İlk M0/M1 işlerinin gerçek süreleri görüldükten sonra süre aralıkları çıkarılır. Şimdiden dayanaksız gün/hafta tahmini verilmez.

## Prototipten yeniden kullanım

| Parça | Yaklaşım | Neden |
| --- | --- | --- |
| Modeller, sesler, materyaller ve Hovl efektleri | Korunur; gereken adaptörle bağlanır | Aynı görsel içeriği yeniden üretmeye gerek yok. |
| Vagon/istasyon yerleşimi ve prefab görselleri | Test sahnesine kontrollü alınır | Kimlik, collider ve nav geometrisi ayrıca doğrulanır. |
| Kamera ve `PlayerMovement` | Uygun kısımları incelenip yeniden kullanılır | Mobil girdi ve konumlandırmayla uyumu görülmeden tümden yeniden yazılmaz. |
| `EnemyHealth` ve `PlayerShootting` | Davranış/görsel referansı; ortak hasar/silah sistemine taşınır | Sabit collision hasarı ve doğrudan girdi bağları geniş hedefi karşılamıyor. |
| `Enemy` hareket/karar mantığı | Yeni brain/motorla değiştirilir | Doğrudan transform hareketi hedef kapı/nav davranışını karşılamıyor. |
| Zamanlı `DoorManager` ve reload kullanan `PlayerSpawn` | Referans prototipte kalır; yeni döngüde değiştirilir | Hedef hasarlı kapı ve kalıcı koşu durumudur. |
| Alternatif/bağlantısız scriptler | Kullanım taraması sonrası ayrı temizlik işi | Aktif sahnede kullanılmıyor diye silinmez. |

## Kapsamı daraltma seçenekleri

İlk denemede tek silah ve tek zombi türü yeterlidir. Kombo, ilave silahlar, gelişmiş turret çeşidi, geniş meta ilerleme ve görsel cilalama ertelenebilir. Turret/dron M3'ün kabulü için gerekli değildir; fakat özellikle dron oyun kimliğinde önemli olduğundan ilk yayın kapsamından otomatik çıkarılmaz. Çekirdek kapı–onarım–zombi–hasar ve rastgele vagon/istasyon döngüsü korunur. Yayın kapsamındaki kesintiler kullanıcıyla kararlaştırılır.

## M0 — Prototipi koru, geliştirme düzenini kur

- Mevcut değişiklikleri ve hangi assetlerin takip edilmediğini kaydet; kullanıcının çalışmalarını toplu commit etme.
- Prototip sahnesini çalışır referans olarak koru. Yeni mekanikler ayrı `NavigationSandbox` / `GameplaySandbox` sahnesinde kurulacak.
- Yeni kodu `Assets/Game` altında başlat; eski scriptleri topluca taşıma/silme.
- AudioListener çoğalmasını küçük, ayrı bir düzeltmeyle gider; yaşayan düşman üzerinde tek hareket sahibi ilkesini hazırla.
- Mevcut projede test çalıştırma/build düzeninin ve Unity bağlantısının nasıl kullanılacağını doğrula. Android cihaz/ölçüm hedefini belirle.

Kabul: eski sahne açılır; test sahnesi açılır; audio listener uyarısı üretimle çoğalmaz; bu düzen mevcut prefab bağlantılarını kırmaz. Referans sahne değiştirildiyse kapsam kayda yazılır.

## M1 — Navigasyon ve portalı kanıtla

- Sabit tren zemini, istasyon zemini ve tek kontrollü giriş portalı kur.
- Kapı açık/kapalı durumu bu aşamada test kontrolünden verilebilir; bu geçici kontrol son oyun mantığı değildir.
- Zombi `NavMeshAgent` ile dış yaklaşma noktasına ve geçit açıkken iç noktaya yürüsün.
- Kapı kapalıyken iç/dış alan arasında başka nav yolu olmadığını doğrula.
- Kapı çevresinde sınırlı saldırı/kuşatma noktaları, yol bulunamama ve nav dışında doğuş durumunu incele.
- Çevrenin görsel hareketinin gameplay collision/nav alanını taşımadığını doğrula.
- Örneğin 10/30/60/100 agent ile kaba ölçek denemesi yap; dar kapı kuyruğu ve açık geçitten geçiş ayrı ölçülsün. Bunlar test yükleridir, hedef canlı sınırı değildir.
- Hedef Android cihazında build/kalite ayarıyla FPS/kare süresi ve CPU maliyetini kaydet. Navigation, physics, animator ve sunum maliyetini ayır. Editördeki FPS cihaz kanıtı değildir. Cihaz yoksa bu kabul maddesi açık kalır; işlevsel denemeye devam edilebilir.

Kabul: kapalı geçitten geçemez; açık geçitten geçer; rota açılıp kapanınca yeniden değerlendirilir; 10 kez durum değişiminde hata/sıkışma yok. Agent ile Rigidbody/root motion aynı transformu sürmez. Nav bake/geometriler ve ilk Android kalabalık ölçümü kaydedilir; cihaz ölçümü eksikse M1 bütünü tamamlandı sayılmaz. NavMesh başlangıç tercihi korunur; ölçülmüş navigasyon darboğazı alternatif motor denemesini gerekçelendirebilir.

## M2 — Ortak sağlık ve hasar

- Oyuncu, zombi, kapı için `Health` ve ortak damage context/result.
- Hasar/heal/max can kuralları, takımlar ve ölüm yaşam döngüsü.
- Oyuncu ölümü ve koşunun bitmesi için açık sinyal; UI bundan bilgi alır.
- Basit tabanca isabeti sadece bu döngüyü doğrulamaya yetecek kadar bağlanır; tam silah sistemi M5'tedir.

Kabul: 10 canlık kapıda 8+2 hasar tam olarak kırar; negatif değerler sınırları bozmaz; 10/100 → 40/100 ve 50/100 → 80/100 iyileşir; maksimum aşılmaz. Aynı karede ölüm/ödül yalnızca bir kez oluşur; ölüye heal otomatik diriltmez. Saf kurallar için anlamlı EditMode testleri.

## M3 — İlk tam oynanış parçası

- `DoorController` sağlık/kırılma/onarım durumunu portal adaptörüne bağlar.
- Normal zombi yaklaşır, kapıya menzilde ve aralıkla vurur, kapı kırılınca girer, oyuncuya saldırır.
- Labda gözlenen vagon içi avoidance tıkanmasını hedef/menzil yerleşimiyle ele al; zombilerin sabit grid hedeflerine dizilmesini gerçek brain davranışı sanma.
- Oyuncu kırık kapı yanında 3 saniye kalınca onarır; uzaklaşınca ilerleme sıfırlanır. Slider gözlemcidir.
- Geçitte zombi/oyuncu varken kapanış, birden fazla onarım hedefi ve aynı anda gelen hasar ele alınır.
- En küçük enemy pool/factory ve kayıt yaşam döngüsü; ölüm/havuza dönüş rezervasyonları temizler.
- Önceden seçilmiş minimum dokunmatik hareket/nişan/ateş adaptörü ve tek tabancayı bağla. Tam silah çeşitliliği M5 kapsamıdır.
- D15/D16: sağlam kapının üst camından dış zombiye atış; alt panel/duvar atışı keser. Oyuncu kapı durumundan bağımsız vagon içinde kalır; hareket alanı daha sonra görev izniyle değişebilir. Zombi girişi bu sınırdan etkilenmez.

Kabul: en az 10 kırılma→giriş→onarım döngüsü; kapı içinde spawn/kapanma yok; onarım iptali can doldurmaz; kırılmış kapıya zombi gereksiz saldırmaz. UI devre dışıyken mekanik çalışır. Oyuncu ölebilir; eski callback yeniden doğurmaz. Pool nesnesi tekrar doğunca eski hedef/hasar/slot taşımaz.

M3 oynanış değerlendirmesi: kullanıcı telefonda kısa oturumlarla hareket–ateş–onarım geçişini, kapı baskısını ve geri bildirimi değerlendirir. Sorunlar kontrol, temel gerilim veya geri bildirim diye kaydedilir. Temel deneyim rahatsız ediciyse M4'e kapsam eklemeden bu parça düzeltilir; keyifli sonucu otomatik test veya editör denemesiyle ilan edilmez.

## M4a — İki vagonla tren ve istasyon akışı

- Önce iki vagonu kimlikleriyle bağla; her kapının ayrı canını ve savunma slotlarını koru. Gerçek turret/drondan önce test durumlarıyla kalıcılık sınırı doğrulanabilir.
- `RunFlow`: yaklaşma → savunma → kalkış uyarısı → kalkış → kararma/atama → yeni yaklaşma. Tek evre sahibi.
- 60 saniye savunma; yeni spawn kesilmesi, yeni istasyon hazırlığı, iç/dış zombi üyeliği ve kontrollü ayrılma.
- Görsel hareket/ivmelenme, ses/kararma adaptörleri; efekt olmasa bile evre akışı çalışır.
- Oyuncuyu tekrar instantiate etmeden taşı; dron sahipliği taşınır, turretler ve kapı hasarı vagonlarda kalır.
- Karanlık oynanış kapısı ve güvenli doğuş politikası; onarım/mağaza gibi eski bağlamları iptal et.

Kabul: en az 10 ardışık istasyon geçişi; etap ilerler; sahne reload gerekmez; oyuncu can/para korunur, kapı canları sıfırlanmaz. İç zombiler taşınır, dış zombiler ödülsüz ayrılır; geçitteki durum çift kaydedilmez. Karanlıkta hasar/ödül/spawn/onarım ilerlemez; dönüşte toplu birikim yok. Her evrede ölüm GameOver'a gider.

M4a oynanış değerlendirmesi: iki vagonun şans/risk farkı, yolculuk temposu ve tamir fırsatı kullanıcıyla denenir. Kullanıcı isterse arkadaş testi yapılır; dışarıya otomatik paylaşım yapılmaz.

## M4b — Beş vagona genişletme

- Kanıtlanan akışı beş vagonun veri/prefab bağlantılarıyla genişlet; istasyon akışını yeniden yazma.
- Farklı dayanıklılıkları, hedef/slot bağlantılarını, kamera ve doğuş güvenliğini doğrula.

Kabul: beş vagonun durumları birbirine karışmaz; atama geçerli konum seçer; düşmanlar doğru üyeliği korur. M4a geçiş kontrolleri yeni geometriyle çalışır. Ek vagonların kalabalık/sunum maliyeti yeniden ölçülür.

## M5 — Kontrolü iyileştirme ve silah çeşitliliği

- M3'teki mobil hareket/nişan/ateş adaptörü geri bildirimle iyileştirilir; girdi gameplayden ayrı kalır.
- Ortak weapon/target/hit/presentation sınırları; tabanca, hafif makineli, tüfek, pompalı tanımları.
- Satın alınmış efektleri gerçek hit logicten ayır; raycast/pellet, menzil, görüş engeli ve uygun hasar hedefleri.
- Erken seçilen tabanca mermi/reload kuralı diğer silahlara genişletilir; yeni türün farklı kuralı kullanıcıyla netleştirilir.

Kabul: silah verisiyle hasar/aralık değişir; sağlam kapının camından atış geçer, dolu alt panel/duvar arkasına isabet olmaz. Aynı atış engeli kuralı oyuncu/turret/dron için kullanılır. Efektin colliderı ikinci hasar oluşturmaz. Pompalı saçması kasıtlı biçimde çoklu isabet edebilir; collider tekrarları sonucu büyütmez. Düşük FPS'de ateş temposu kontrolden çıkmaz. Mobilde hareket+ateş+onarım birlikte oynanabilir.

## M6 — Spawn ve enemy ölçeği

- Gerçek tren/altı kapılı vagon entegrasyonunu ayrı küçük işle doğrula: kapı seçimi, kapı/istasyon spawn noktaları ve güvenli iç noktalar geometri verisinden gelsin. M4b'nin tek-kapılı lab varsayımlarını üretim geometrisine taşımadan kaldır.
- `StationDefinition` ile vagon/tür/zaman/bütçe dağılımı; Inspector'dan ayarlanır.
- Normalden türeyen hızlı/dayanıklı enemy profilleri; ortak brain/motor/hasar.
- Global/vagon canlı sınırları, spawn istek kuyruğu sınırı, kare başına üretim bütçesi.
- Havuzların ısınması/sıfırlanması, hedef kayıtları, dağıtılmış AI/path yenilemesi.
- Taşınan içerideki düşmanlar canlı limitine dahil; dış kalanlar eski istasyonla temizlenir.

Kabul: yanlış veriler anlaşılır tanıyla reddedilir; geçersiz nav spawn sonsuz denenmez. Her zombi doğru vagona atanır. Uzun denemede canlı nesne/pool/subscription sayıları kontrolsüz büyümez. Global sınır aşılmaz; dolu kapasitede spawn zamanlayıcısı kilitlenmez. Hedef Android cihazında kalabalık profili çıkarılır.

## M7 — Ekonomi ve savunmalar

Bu aşama üç küçük parçaya bölünür:

1. Cüzdan/ölüm ödülü/mağaza işlemi; heal, mevcut vagon kapı onarımı ve silah satın alma.
2. Sabit slotlu normal/gelişmiş turret; ortak silah, görüşü açık en yakın hedef, mermi ve kurulum sınırı.
3. Tek dron; yumuşak takip/uçuş sunumu, oyuncuyu koruma, mermi bitişi ve kamikaze.

Kabul: aynı ölüm tek ödül; çift dokunma çift ücret/ürün üretmez. Dolu slot/tam can/sınır/yetersiz para satın alımı para kaybettirmez. Toplu tamir yalnızca güncel vagonu etkiler. Turretler vagonlarında kalır ve görüş engelini önemser. Dron oyuncuyla taşınır; hedef ölürse/kalmazsa kilitlenmez; patlama/ödül bir kez uygulanır. Ürün davranışları UI kapalıyken de çalışır.

## M8 — Denge ve kayıt

- İlk birkaç istasyonun tür/tempo/ödül/fiyat dengesi; canlı limitini sürekli büyütmeden zorluk artışı.
- Vagon şansı, güvenli doğuş, ücretsiz onarım ve eski vagon turret gelirlerini birlikte oynayarak değerlendir.
- Erken kararlaştırılan koşuya devam/meta ilerleme kapsamında gereken kayıtları uygula. Versiyonlu şema yalnızca kullanılan kayıt özelliklerinin ihtiyacına göre geliştirilir.
- Sonuç ekranı, küçük öğretici ve kontrol geri bildirimi. Kombo sistemi çekirdek oturduktan sonra ayrı tasarım olarak eklenir.

Kabul: ayarlar kod değiştirmeden çalışır; kayıt yükleme sınırları/bozuk kayıt güvenle ele alınır; restart koşuyu doğru sıfırlar. İstasyondan istasyona kalan state ile yeni koşu state'i karışmaz. Oyun dengesi yalnızca otomatik testle değil Play/cihaz deneyimiyle değerlendirilir.

## M9 — Performans, reklam/IAP ve yayın hazırlığı

- CPU/GPU, pathfinding, fizik, animator, ses, ışık/VFX, GC ve bellek ayrı profillenir.
- Hedef cihazlarda 15–30 dakika oturumlar; ısı/batarya etkisi ve gerçek maksimum kalabalık senaryosu.
- Aktif düşman, corpse, projectile, ışık, ses ve parçacık bütçeleri ölçüme göre ayarlanır. Görünmeyen vagonlarda sunum maliyeti azaltılır; oynanış kuralları korunur.
- Reklam ve IAP servisleri gameplayden ayrı adaptörlerde; ürün teslimi, iptal/hata ve tekrar callback davranışları doğrulanır.
- Google Play sürüm hazırlığı ve güncel gereklilik kontrolü bu aşamada ayrıca yapılır; bugünden yayın koşulları sabit varsayılmaz.

Kabul: seçilen cihaz/kalite profili/FPS ve canlı sınırı raporlanır; ölçüm olmadan “kusursuz optimize” kabulü yoktur. Yayın/reklam yerleşimi kullanıcıyla somut build üzerinden değerlendirilir.

## Her aşamada tekrar geçerli olan kontroller

- Can/hasar/heal ve tek ölüm/ödül gibi saf kurallar için otomatik test; ilk hareket/kapı/onarım kabulü küçük sahnede elle Play kontrolü. PlayMode otomasyonu tekrar eden veya riskli senaryoya gerçekten fayda sağladığında eklenir; her özelliğe zorunlu test yazılmaz.
- Console'da yeni hata veya sürekli uyarı oluşmaması.
- Pause, oyuncu ölümü, hedefin yok olması ve tekrar başlatma ilgiliyse kontrol edilir.
- Havuz kullanılan işlerde eski callback/kuşak/abonelik temizliği o özellik eklenirken kontrol edilir; bu doğrulama kalite aşamasına ertelenmez.
- Doğrulama sonucu ve bilinen sınırlar kayda yazılır; tüm testler her küçük değişiklikte gereksiz yere yeniden koşturulmaz.

## Bir sonraki somut iş

**M5 değerlendirmesi:** TrainSandbox'ta dört silahı, sınırlı mermiyi ve otomatik doldurmayı kullanıcıyla gözle. Sonraki M6'nın ilk küçük işi gerçek altı kapılı vagon geometrisi, kapı seçimi ve açık spawn/güvenli iç nokta verileri; ardından veri temelli spawn bütçesi ve enemy profilleri. Ortak silahı gerçek turret/dronda kullanma M7'de doğrulanır. Android kontrol/efekt maliyeti uygun zamanda ayrıca kontrol edilecek.
