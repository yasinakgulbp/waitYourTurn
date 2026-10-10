# Sıralı geliştirme planı

**D50 güncel tercih (2026-10-10):** Kullanıcı önceki tren modelini yüzey dokusuz kullanmak istedi; D48 meshleri düz malzemelerle korunur. Yeni doku fikri beklenir; D49 lookdev çalışma olarak saklanır, oyuna kurulmaz. Önceki malzeme denemesine kullanıcı onayı var sayılmaz. Ayrıntı [TRAIN_ART.md](TRAIN_ART.md).

**D49 güncel sanat işi (2026-10-10):** Kullanıcı D48 görünümünü reddetti. Önce özgün Blender malzeme/ışık denemesi; native node, gerçek model renderi ve renk örnekleri `ArtSource/SurvivalTrain/LookDev` içinde. Unity sahnesine bu çalışmada kurulum yoktur. Görsel yön değerlendirmesinden sonra kompakt bake ve tek-vagon Unity eşleştirmesi yapılır; bütün treni tekrar körlemesine giydirme yapılmaz. İç dekor şu an ayrı görsel öneridir. A54 sanat yükü ölçümü bekler; Blender renderi mobil kabul değildir. Ayrıntı [TRAIN_ART.md](TRAIN_ART.md).

**D47 sanat devam noktası (2026-10-09):** İlk sarı çizgili taslak reddedildi; referans malzemeleriyle yeni üç vagonlu Blender gövdesi hazırlandı. Kullanıcı önizlemeden sonra ilk denemeyi Survival'a kurmamızı istedi. Kurulum ve kısa cam/metal/15 kapı/iki körük PC kabulü PASS; hareket/nav korunur, Battle sahnesi değişmedi. D48 revizyonunda kaynak,24.660 üçgen ve39 açık tren rendererı [TRAIN_ART.md](TRAIN_ART.md) içinde. Sonraki adım kullanıcı oyun görünümü değerlendirmesi ve A54 sanat yükü karşılaştırması; D48 ile nötr çelik/sakin zemin, körük yüzey örtüşmesi ve tek ışık revizyonu uygulandı; yeni görünüm değerlendirmesi açık. Cihaz kabulü henüz tamamlanmış değildir.

**2026-10-09 D37/D39 revizyonu:** M8b güncel A54 temeli → M8s Solo açma/geçiş/spawn/kayıt → M8e minimum kalıcı sonuç/harcama → M8c geri bildirim/UI/öğretici → M8d iki mod oyuncu denemesi. M8s fiziksel geçiş PC'de hazır; A54 geçiş kabulü kullanıcı isteğiyle bekliyor, yeni vagon ganimeti açık. M9a tek-vagon sanat örneği güncel geçit ölçüleriyle M8c yanında hazırlanabilir. Her fiziksel/kayıt değişimi ayrı küçük commit ve uygun Android kontrolüyle ilerler. Yeni mod/ekonomi [PROGRESSION_DESIGN.md](PROGRESSION_DESIGN.md); aşağıdaki tarihsel Solo rastgele atama/önceki meta notlarının yerine güncel hedef geçer. Kalıcı ekonomi/sonuç tasarımı henüz uygulanmadı. İlk yayın iki mod hedefi korunur.

Güncel durum: 2026-10-09 — ortak savaş/kapı, beş vagonlu iki taraflı geometri, spawn, mağaza, turret, dron ve dört bot PC teknik kabulüne ulaştı. M8a yerel kayıt PC'de doğrulandı; D35 kalabalıkta oyuncu sınırı ve yumuşak dönüş düzeltmesi geçti. D38 ile birleşik A54 yatay açılışı, joystick izi düzeltmesi ve 10 dakika/60 canlı düşman sınırında sentetik kablolu Battle yükü doğrulandı: ortalama 59,97 FPS, termal durum normal, normal koşu kaydına dönüş aynı runId ile görüldü. [A54_PERFORMANCE.md](A54_PERFORMANCE.md). Pil tüketimi, iki mod tam dokunma/ölüm kabulü, son sanat/yayın benzeri uzun oturum ve AssetPackManager açılış hatası açık. M8b kısmi kabulden sonra D37/M8s Solo fiziksel ilerleme gelir; diğer açık Android kapıları yayın öncesinde kapanır. Ayrıntılı sıra [RELEASE_PLAN.md](RELEASE_PLAN.md) içindedir. Aşağıdaki tarihli laboratuvar kayıtları geçmiş kanıttır; güncel yapılacaklar listesi olarak okunmaz.

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
| M6a | Hedef vagon/kamera ve iki taraflı çok kapı entegrasyonu | M3–M5 temel kuralları | İlk altı-kapılı entegrasyon PASS. D23 yeni oran/kapsül/4–6 kapı revizyonu da beş-vagonda PASS: 10 geçiş, durum kalıcılığı ve 60 düşman sınırı. 38 can/onarım/geometri/yerleşim + 15 yolculuk/kamera testi PASS; 16:9/4:3 Editor kadrajı görüldü. Kullanıcı his ve Android değerlendirmesi açık. VISUAL_SCALE.md, TRAIN_INTEGRATION.md |
| M6 | Veriyle yönetilen spawn, enemy çeşitleri ve kalabalık bütçesi | M6a kabulü | PC teknik kabulü: üç ortak profil, veri programı ve sınırlı director kuruldu; 26 kural/yolculuk testi, 30 istasyon/5 yolcu/60 sabit havuz ve eski birleşik kabul PASS. Android kalabalık profili henüz yapılmadı; SPAWNING.md |
| M7 | Para, mağaza, turret ve dron | M2, M4–M6 | M7a/M7b/M7c PC teknik kabulü PASS; ECONOMY.md, DEFENSES.md, DRONE.md. Mod/Bot artık uygulandı; mobil ergonomi ve denge açık |
| Mod/Bot | Battle için komşu vagon botları, en az 3 beceri profili ve eleme/sıralama; botsuz ilerleme modu politikası | M4b ortak döngü, M5/M6 savaş, M7 ekonomi | İlk PC kabulü PASS: 4 bot / 3 profil, ortak savaş/onarım/ekonomi, eşsiz atama, eleme/sıralama ve Solo; BOTS.md. Uzak simülasyon/Android/uzun denge açık; gerçek multiplayer yok |
| M8 | Etap dengesi, kayıt ve koşu deneyimi | M4–M7 | M8a PC kayıt temeli PASS; M8b Android → M8s Solo geçiş/açma → M8e minimum meta/sonuç → M8c hafif kamera/ses/UI → M8d iki mod oyuncu değerlendirmesi. RELEASE_PLAN.md, PROGRESSION_DESIGN.md |
| M9 | Sanat, mobil kalite, gelir ve yayın doğrulaması | M8 değerlendirmesi | M9a tek vagon sanat örneği → M9b bütçeli içerik/denge → M9c gelir/ölçüm adaptörleri → M9d kapalı ve sınırlı yayın → M9e ölçümlü genel yayın. RELEASE_PLAN.md |

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

## M6a — Gerçek vagon, kamera ve iki taraflı giriş

2026-10-08 D22 ile ayrı kabul olarak öne alındı. Referans `Assets/Scenes/SampleScene 2.unity`; kare/tek kapılı labın görünümü ürün hedefi değildir. Kod denetimi ve bileşen bazında uyarlama listesi `VISION_AND_INTEGRATION.md` içindedir.

- Mevcut labları koru; referans oranlarıyla uzun dikdörtgen vagon, altı kapı, iki taraflı istasyon zemini ve oyuncuyu izleyen kamera kur. Kamera hareketinin tek sahibi olsun; oyuncu yürüyüşü ve vagon ataması birlikte doğrulansın.
- Kapı seçimini tek havuz kapısı varsayımından ayır. Portal eşiği ile gerçek vagon iç hacmi/üyeliğini ayır; karşı istasyon yarı düzlem nedeniyle içerisi sayılmasın. Kapı bekleme ofsetleri portalın yerel eksenlerine uysun.
- Doğuş ve güvenli iç adaylar gerçek geometriye ait açık çapalar/veriden gelsin. Collider/MovementArea/NavMesh yeni yerleşime göre kurulsun; eski baked lab verisi ölçeklenmiş modele geçerli sayılmasın.
- Önce bir vagonda iki yönden saldırı/altı kapı; sonra en az iki gerçek oranlı vagonda istasyon geçişi/kalıcılık. Ardından beş vagona uygula. Yeni enemy türü veya ekonomi sistemi bu kabulü kapatmanın parçası değil.

Kabul: iki tarafta eşzamanlı doğru doğuş/rota, bağımsız kapı canı ve hasar/onarım/giriş; tüm kapılarda cam/duvar/alt panel atış kuralı; oyuncunun içeride kalması; iki yöndeki dış/iç/eşik kalkış kararları; güvenli atama ve kamera takibi; sabit gameplay zemini/ayrı görsel hareket; havuz sınırı ve durum kalıcılığı. Referansa yakın kadraj kullanıcıyla değerlendirilir. Bu kabul geçmeden yeni mağaza/turret/bot geliştirmesine geçilmez.

## M6 — Spawn ve enemy ölçeği

- M6a/D23 ile doğrulanan iki taraflı/4–6 kapılı geometri verilerini kullan; M4b'nin tek taraflı/tek kapılı lab varsayımlarına geri dönme.
- `StationDefinition` ile vagon/tür/zaman/bütçe dağılımı; Inspector'dan ayarlanır.
- Normalden türeyen hızlı/dayanıklı enemy profilleri; ortak brain/motor/hasar.
- Global/vagon canlı sınırları, spawn istek kuyruğu sınırı, kare başına üretim bütçesi.
- Havuzların ısınması/sıfırlanması, hedef kayıtları, dağıtılmış AI/path yenilemesi.
- Taşınan içerideki düşmanlar canlı limitine dahil; dış kalanlar eski istasyonla temizlenir.

Kabul: yanlış veriler anlaşılır tanıyla reddedilir; geçersiz nav spawn sonsuz denenmez. Her zombi doğru vagona atanır. Uzun denemede canlı nesne/pool/subscription sayıları kontrolsüz büyümez. Global sınır aşılmaz; dolu kapasitede spawn zamanlayıcısı kilitlenmez. Hedef Android cihazında kalabalık profili çıkarılır.

## M7 — Ekonomi ve savunmalar

M7b öncesi 2026-10-08 kullanıcı denetimi: boş/elenmiş savunmacı vagonuna yeni üretim kapatılır; içerideki kalanlar korunur. Güvenli atama en uzak boş adayı seçer, ayarlanabilir 2 sn hasar koruması ateş/onarımı durdurmaz. Bu düzeltmenin PC kabulü yeni turret geliştirmesinden önce kayda alınır. Tamamlanmış kabul olmayan sürüm PASS sayılmaz.

Bu aşama üç küçük parçaya bölünür:

1. **M7a PC teknik kabulü geçti:** cüzdan/ölüm ödülü/mağaza işlemi; heal, mevcut vagon kapı onarımı ve silah satın alma/refill. Savaşta gerçek zamanlı mağaza, karanlık/ölümde kapalı; ilk silah alımı açar, tekrar alım eksik mermiyi aynı fiyatla tamamlar. 11 ekonomi + 15 silah testi ve F9/F10 sahne kabulleri PASS; ayrıntı ECONOMY.md.
2. **M7b PC teknik kabulü geçti:** oyuncunun bulunduğu konuma kurulan normal/gelişmiş turret; ortak silah, görüşü açık en yakın hedef, 50/80 mermi, vagon başına tür başına 2, tek bitiş patlaması/ödül. 20 silah/patlama + 12 ekonomi testi ve F12 sahne kabulü PASS; DEFENSES.md.
3. **M7c PC teknik kabulü geçti:** tek yeniden kullanılan dron; yumuşak takip/salınım, ortak atış/ödül, sonlu mermi, engel kontrollü kamikaze ve oyuncuyla vagon kalıcılığı. Ayrıntılar DRONE.md.

Kabul: aynı ölüm tek ödül; çift dokunma çift ücret/ürün üretmez. Dolu slot/tam can/sınır/yetersiz para satın alımı para kaybettirmez. Toplu tamir yalnızca güncel vagonu etkiler. Turretler vagonlarında kalır ve görüş engelini önemser. Dron oyuncuyla taşınır; hedef ölürse/kalmazsa kilitlenmez; patlama/ödül bir kez uygulanır. Ürün davranışları UI kapalıyken de çalışır.

## M8 — Denge ve kayıt

**2026-10-09 M8a:** yerel koşu snapshot'ı, atomik dosya/yedek, canlı katılımcı ve vagon durumları, eleme/mermi/para/spawn/RNG devamı uygulanır. Kapsam ve PC kanıtı [PERSISTENCE.md](PERSISTENCE.md). Sonraki M8b: Android arka plan/süreç kapatma/yeniden açma kabulü, devam/sonuç arayüzü ve ilk istasyon dengesi. Bulut ve kalıcı ekonomi kararı ayrıca verilir; bu temel M8'in tamamı değildir.

M7a sonrası gelir taslağı [MONETIZATION.md](MONETIZATION.md) içinde: kullanıcının Battle sonuç 3×, hikâyede devam/3× ve zorunlu reklam kaldırma fikirleri; araştırılan Bullet Echo/The Tower örnekleri ve sade ekonomi önerisi. Koşu parası ile kalıcı sonuç ödülü ayrılır; premium para, meta harcamaları, ürün fiyatları ve reklam sıklığı henüz seçilmedi. Tek teslim ve uygulama kapanması sınırları kayıt/sonuç tasarımında ele alınır.

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

## Önceki aşamaların tarihsel geçiş kayıtları

D23 ölçek/kadraj revizyonu M6 öncesine eklendi: yeni üç kullanıcı referansına yakın blok vagon, 1,7 boyunda kapsüller, 4–6 kapı ve responsive perspektif. Altı kapı sabiti sonraki verilerde varsayılmaz; M6 doğuş/kapı listelerini mevcut geometriden okur. Güncel ölçüler ve revizyon kanıtı VISUAL_SCALE.md içindedir.

D24 revizyonu tamamlandı: iki taraftaki yan camlar atışı geçirir, metal dikme/çerçeve/paneller engeller; pencere zombi geçidi değildir. 39 kural testi ve 46 pencerenin dört silahla kontrol edildiği beş-vagon/10-geçiş/60-düşman PC kabulü PASS (2026-10-08 08:03:45 UTC). Model ölçüleri MODEL_CONTRACT.md ve üretilen CSV'de tutulur. Sonraki M6 veri/bütçe sistemi bu sabit geometriyi kullanır; pencere sayısını kapı/spawn sayısı gibi saymaz.

**Güncel M6 durumu:** veri programları, üç ortak zombi profili, global/vagon/kare sınırları, sonlu doğuş denemesi ve kontrollü havuz ısınması PC’de doğrulandı. İçeridekiler canlı limitine dahil. Kod/Inspector bağlama, ilk test değerleri ve yeniden çalıştırma bilgileri [SPAWNING.md](SPAWNING.md) içinde. Android performansı ve gerçek uzun oturum açık; kısa PC kabulü bunların kanıtı değildir.

**M7b sonrası tarihsel M7c geçiş kaydı:** M7b sabit turret PC kabulü geçti; normal/gelişmiş tür, vagon kotası, ortak cam/metal atışı, bitiş patlaması, kalıcılık ve oyuncu ödülü doğrulandı. Ayrıntı [DEFENSES.md](DEFENSES.md). Sonraki iş tek oyuncu dronu: takip/uçuş, ortak hedef/atış, mermi bitişinde kamikaze ve oyuncuyla vagon değişimi. Turret yerinde kalmaya devam eder. Öncesindeki D27/D28 kapısı PC'de geçti: 68 kural testi, boş/elenmiş vagon üretimi ve koruma süresiyle F11 30 geçiş, F9 mağaza ve F10 önceki çekirdek PASS; [kanıt](generated/occupancy-arrival-pc-acceptance.txt). M7a kuralları/ayarları [ECONOMY.md](ECONOMY.md) içinde. M7b → M7c dron → Mod/Bot sırası korunur. Kullanıcının reklam/IAP fikirleri ve kaynaklı alternatifler [MONETIZATION.md](MONETIZATION.md) içinde; kalıcı ödül/ürün/fiyat henüz seçilmedi ve servis eklenmedi. Android ergonomisi ve ekonomik denge sonraki değerlendirmede açık kalır.

M7b son durum: PC kanıtı generated/m7b-pc-acceptance.txt; Android ve uzun oturum açık. D29 kararları uygulanır. D30 ile Unity Ads tercih edildi, IAP ekonomisi ileride seçilecek; SDK entegrasyonu yapılmadan oynanışa devam edilir. Sonraki M7c → Mod/Bot (en az üç beceri profili) → M8 → M9 sırası korunur.

D31 kullanıcı revizyonu: sabit padler kaldırıldı, turret oyuncunun bulunduğu konuma kurulur; daha küçük fizik/nav alanı ve sahibin içinden yürüyebilmesi. Eşit 2,1 m duvarlar, ince üst/geniş dikey pencere metali, sabit kapı üst çerçeveleri; kuyruğa doğru azalan dayanıklılık ve kapalı lokomotif. Normal oyunda yeni koşu tohumu/rastgele ilk vagon, denemeler için geçici 1000 para. Güncel sözleşme MODEL_CONTRACT ve DEFENSES içinde. Sonraki mekanik hâlâ M7c dron; revizyonun PC kanıtı generated/free-placement-train-pc-acceptance.txt içinde tutulur.

**M7c sonundaki tarihsel sonraki iş: Mod/Bot.** M7c D32 ile PC kabulüne ulaştı; takip/atış/mermi/kamikaze/kapı/kalıcılık/ölüm ve yeniden kullanım doğrulandı. Ayrıntı [DRONE.md](DRONE.md), kanıt generated/m7c-pc-acceptance.txt. Botlar henüz uygulanmadı; en az üç beceri profili ve komşu vagonlarda ortak savaş/onarım kuralları sıradadır. Yukarıdaki M7c sıradaki iş ifadeleri önceki adımların tarihsel kaydıdır.

**Mod/Bot sonundaki tarihsel sonraki iş: M8 kayıt.** Mod/Bot ilk PC kabulü PASS: beş vagon / insan + dört bot / üç beceri profili; aynı savaş, onarım, fiyat ve ödül kuralları. D33 sonuç politikası kullanıcı tarafından seçildi. Tüm yaşayan katılımcıların eşsiz güvenli ataması, eleme, para/mermi/kimlik kalıcılığı, botsuz mod ve yeniden kullanım doğrulandı. Ayrıntı [BOTS.md](BOTS.md), kanıt generated/bots-pc-acceptance.txt. Bu aşamanın kullanıcı oynanış değerlendirmesi, Android/uzun yarış dengesi ve uzak simülasyon optimizasyonu açık; PC teknik kabulü bunların yerine geçmez. M8 insanla birlikte bot can/para/mermi/eleme/vagon ve RNG devamını da kaydetmeli; ölüm/birincilik sonrası kayıt eski koşuyu diriltmemeli. M9 servisleri ve IAP kararları ertelenmiş olarak kalır.

**Güncel küçük parça: M8s sandık + M8e minimum profil/güç.** Solo geçişi, tek cephane sandığı, ayrı kalıcı cüzdan, sonuç ödülü, üç silahın kalıcı açılışı ve sınırlı can/menzil geliştirmesi PC birleşik kabulünden geçti. D40 ile Solo ölümde ilk istasyon/kuyruktan yeni challenge; canlı koşuya ara verme kaydı korunur. Kanıt `generated/permanent-upgrades-pc-acceptance.txt`, ayarlar/sınırlar PROGRESSION_DESIGN.md. Sıradaki M8c parçaları sade oyuncu menüsü/başla-devam-sonuç, hafif kamera/ses/geri bildirim ve kısa kullanıcı denemesidir. A54 birleşik Solo/profil/güç kontrolü hâlâ bekliyor; gelir/bulut SDK eklenmedi. Sayısal ekonomi/keyif dengesi veya M8 cihaz kabulü bitmiş sayılmaz.

## Güncel D44 devam noktası

Survival orta başlangıç/4→7→10/+3 dalga ve eşit20HP kapıları, sade HUD/fonsuz mini harita, katalogdan kaydırılabilir mağaza grupları, tüm dış kapılara koşu içi tahta/tel güçlendirme uygulanır. Yeni koşuda seviye/kapıHP sıfırlanır; kayıt bunları tutar. Damage onarım kesintileri ortak varsayılanda kapalıdır. Ayrıntı DECISIONS D44 ve PROGRESSION_DESIGN. Kalıcı RepairSpeed sonraki profil/menü görevinde iki mod için bekliyor. A54, sanat ve nihai denge kabulü tamamlanmış sayılmaz.

### D45 — fiyatlar ve patlayıcı ekipman

Survival fiyat/3000 test altını güncellendi; ortak bomba atar teslimi ve düşman tetiklemeli mayın mevcut Combat/ödül/kayıt sistemleri üzerinden kuruldu. Ayrıntılar: DIFFICULTY_AND_PRICES.md. Sonraki denge işi olarak belirlenen tehdit bütçesi D46'da uygulandı;4/7/10 başlangıcı korunur. Battle'a dönüldüğünde launcher+mine sahne bağları, bot kullanımı ve ayrı ekonomi kabulü eklenecek. Launcher'ın kalıcı silah kilidi/profil göçü ayrı iş; kalıcı hızlı onarım iki mod için bekler. Android patlayıcı ekipman kontrolü beklemede.

### D46 — güncel devam noktası: mühimmat, nişan ve dalga bütçesi

Ortak el silahları dolu+7 yedek şarjör; pompalı7m, launcher12m/24m/s ve hareketli hedef tahmini. Survival'da tek silah satın alma + ucuz bir şarjör ürünü/HUD düğmesi. Dördüncü istasyondan13/+3/max160 tehdit bütçesi, hızlı5/dayanıklı8, üç vagona tek toplam bütçe; canlı/kuyruk/havuz sınırları korunur. Kayıt içerik anahtarı eğriyi denetler. Ayrıntılar DIFFICULTY_AND_PRICES, kanıt generated/survival-ammo-aim-budget-pc-acceptance.txt. 10000 altın ve kalıcı silah kilidi bypass yalnız deneme ayarıdır.

Sonraki somut iş kullanıcıyla Blender üç vagon/lokomotif görselini mevcut MODEL_CONTRACT/ölçü CSV'lerine göre tasarlamaktır. Renderer giydirme, oynanış collider/nav/atış açıklıkları ve kapı kimliklerinden ayrı tutulur. İlk giydirilen vagonla kısa mekanik/cihaz kontrolü yapılır; bütün sanatı bitirip sonra uyumu denemeyiz. Nihai denge, kalıcı hızlı onarım, üretim menü/öğretici, Battle ekipman entegrasyonu ve yayın hazırlığı açık görevdir; bu aşama bunları tamamlamış sayılmaz.
