# Karar kaydı

Tarih: 2026-10-07. `Kararlaştırıldı` kullanıcı tarafından netleştirilen tasarım veya görev kapsamıdır. `Teknik öneri` uygulanmadan önce ilgili aşamanın testinde doğrulanır; kullanıcı onayıymış gibi sunulmaz.

## Kararlaştırılanlar

| Kimlik | Karar | Gerekçe / etkisi |
| --- | --- | --- |
| D01 | Modüler, az bağımlı mimari; küçük ve doğrulanmış aşamalar | Ana mekanikler birbirine uyumlu gelişecek; tek seferde tüm oyun yazılmayacak. |
| D02 | Gameplay tren zemini sabit; çevre görsel olarak hareket eder | Kullanıcı seçti. Hareketli platformun navigasyon/physics yükü ilk kapsamda yok. |
| D03 | İlk istasyon savunması 60 saniye; süre veriden değişir | Kullanıcı seçti. Tüm zombilerin ölmesi beklenmez. |
| D04 | İçeri giren zombiler trende kalır; dışarıdakiler istasyonda kalır | Kullanıcı seçti. Yolculuğun görünen bölümünde tehlike devam eder; içeride/dışarıda üyelik takip edilir. |
| D05 | Can, para, silah ve dron oyuncuyla gider; kapı hasarı/turretler vagonlarında kalır | Kullanıcı seçti. Vagonların durumları ayrı ve koşu boyunca kalıcıdır. |
| D06 | Karanlık geçişte oynanış durur; görünür yolculukta savaş sürer | Kullanıcı seçti. Kontrol/görüş olmayan sürede haksız hasar ve kaynak üretimi yok. |
| D07 | İyileştirme mevcut canın %50'si, minimum 30; maksimum canla sınırlı | Kullanıcı seçti. 10/100 → 40/100, 50/100 → 80/100. |
| D08 | Kapılar hasarla kırılır; vagonlara göre maksimum canları farklıdır | Kullanıcının ana mekaniği. Prototipteki zamanlı kapılar yeni davranışla değiştirilir. |
| D09 | Kırık kapıya yakın kalınca yaklaşık 3 saniyede onarım; ayrılınca iptal | Kullanıcının ana mekaniği. Süre ayarlanabilir; yarım onarım kapıyı tamamlamaz. |
| D10 | Turret ve dron sınırlı mermi kullanır; dron oyuncuyu izler ve mermi bitince kamikaze yapar | Kullanıcının ana mekaniği. Satın alma sınırları ve hedef kaybı yönetilir. |
| D11 | İngilizce, aşamalara ait küçük commitler; repo içi ilerleme kaydı | Kullanıcının çalışma tercihi. Kaynak koddaki mevcut değişiklikler plan commitine alınmaz. |
| D12 | Dış incelemeler öneridir; değişiklikler oyun hedefi ve somut gerekçeyle seçilir | Kullanıcının tercihi. Model uzlaşması veya nezaket tasarım kararının kanıtı değildir. |
| D13 | Sol joystick ile hareket, en yakın görüşü açık zombiye otomatik nişan ve ateş | Kullanıcı M3 için seçti; masaüstünde WASD/ok tuşları aynı hareket girdisine bağlanır. Sağ joystick gerekmez. |
| D14 | İlk tabanca sınırlı şarjör, sınırsız yedek mermi ve otomatik doldurma kullanır | Kullanıcı M3 için seçti. İlk lab değerleri deneme ayarıdır: 8 mermi, 1.2 s doldurma, 0.35 s atış aralığı. |
| D15 | Sağlam kapının üst camından dışarıdaki zombilere ateş edilir; alt dolu panel ve vagon metalleri atışı keser | Kullanıcı M3 değerlendirmesinde netleştirdi. Oyuncu ve ileride kapı önündeki turretler zombileri kapı kırılmadan vurabilir. Kapının yürüyüş engeli ile atış engeli ayrıdır. Başlangıçta tüm yan duvar opaktı; D24 yan pencere açıklıklarını ayrıca geçirgen yapar. |
| D16 | Oyuncu ilk kapsamda atandığı vagondan dışarı yürüyemez; kapı kırılması bu izni değiştirmez | Kullanıcı M3 değerlendirmesinde seçti. Hareket alanı ayrı ve isteğe bağlıdır; ileride istasyona inme görevi bu sınırı kaldırabilir/değiştirebilir. Zombi girişini veya atışı engelleyen ek fizik duvarı kurulmaz. |
| D17 | Kayıt hedefi mevcut koşuya devam, ölümde yeni koşu; kalıcı geliştirmeler ayrıca tasarlanır | Kullanıcının ilk seçimi; ardından sağlayıcı/ayrıntılarda emin olmadığını belirtti. Google Play Games Saved Games adaydır, kesin sağlayıcı seçilmedi ve M4'te disk/bulut entegrasyonu yapılmaz. |
| D18 | Ortak temel üzerinde botlu hayatta kalma yarışı (Battle) ve botsuz tek oyunculu ilerleme/hikâye modu hedeflenir | Kullanıcının yeni vizyonu: tren giriş sunumu, nickli komşu vagon rakipleri, son hayatta kalan/sıralama; en az 3 bot beceri profili. Gerçek ağ oyuncuları bu talepten çıkarılmaz. M4a önce ortak döngüyü kanıtlar; mod/bot aşaması ayrı kurulur. |
| D19 | Tabanca sınırsız yedek; hafif makineli, tüfek ve pompalı toplam sınırlı mermi kullanır | Kullanıcı M5'te seçti (2026-10-08). Şarjör bitince yedek varsa otomatik doldurma; son dolum kısmi olabilir. Yeni silah alma/mermi ürünü M7'de tasarlanır; silah veya vagon değiştirmek mermi vermez. |
| D20 | Varsayılan: oyuncu hasarı onarımı kesmez, kapı hasarı ilerlemeyi sıfırlar; iki kural ayrı ayarlanabilir | Kullanıcı 2026-10-08'de bu ayrımı netleştirdi. Hasar engellenmez; kapı kırılabilir ve oyuncu ölebilir. Onarım süresi/menzili ve iki kesilme anahtarı denge araçlarıdır. Dalga bitişi koşulu yoktur. |
| D21 | Zombiler trenin iki tarafından da gelebilir | Kullanıcı 2026-10-08'de hatırlattı. Tek taraflı/tek kapılı lab üretim hedefi değildir. Gerçek vagon entegrasyonunda her iki tarafın yürünebilir alanı, kapıları ve spawn verileri doğrulanır; sunum hareketi saldırı tarafına bağlı olmaz. |
| D22 | Nihai görünüm/yerleşim referansı ilk prototiptir; lab sahnesi onun yerine geçmez | Kullanıcı 2026-10-08'de açıkça yeniden belirtti: uzun dikdörtgen vagon, oyuncuyu izleyen prototip kamera yaklaşımı, iki tarafta istasyon alanı ve çok kapı. M6a gerçek geometri/kamera entegrasyonu yeni özelliklerden önce doğrulanır. Somut lab varsayımları VISION_AND_INTEGRATION.md içinde. D23 yeni kamera/ölçek görselleriyle oranları günceller; eski prefab ölçüleri zorunluluk değildir. |
| D23 | Yeni üç görsel kamera/ölçek/atmosfer referansı; vagon başına 4–6 kapı ve aynı gövde ölçüsü | Kullanıcı 2026-10-08: eski prototipteki oransız boyutlar korunmasın, yönü belli kapsüller kullanılabilir; görsellerde yalnız tarif edilen özellikler referanstır. Kapı sayısı ve dayanıklılık ayrı şans/zorluk araçlarıdır; ortadaki bir/iki kapı duvarla değişebilir. Telefon/tablet kadrajı uyarlanır. Deneme ölçüleri, model bağlama ve kanıt VISUAL_SCALE.md içindedir. |
| D25 | Mağaza savaşta ve görünür yolculukta kullanılabilir; açıkken oyun sürer | Kullanıcı M7a'da seçti. Karanlık geçişte ve ölümde satın alma kapanır; vagon değişimi eski panel bağlamını geçersiz kılar. |
| D26 | Silah ilk alımda açılır ve dolar; tekrar satın alma eksik mermiyi aynı fiyatla tamamlar | Kullanıcı M7a'da seçti. Doluyken para harcanmaz; satın alma atış beklemesini sıfırlamaz. Tabanca ücretsiz başlangıç silahıdır. |
| D27 | Yaşayan insan/bot savunmacı olmayan vagona yeni zombi üretilmez; içeride kalanlar korunur | Kullanıcı M7a değerlendirmesinde boş/elenmiş vagonların her istasyonda dolup kaçınılmaz ölüm yaratmasını istemedi. Uygunluk görünürlüğe veya sadece insanın seçili vagonuna bağlanmaz. |
| D28 | Atamada zombilerden en uzak fiziksel boş aday + ayarlanabilir 2 sn hasar koruması | Kullanıcı soruda onayladı. Karanlıkta süre tüketilmez; ateş/onarım sürer. Sayısal denge ileride değişebilir. |

## Teknik öneriler

| Kimlik | Öneri | Doğrulama aşaması |
| --- | --- | --- |
| T01 | Yakın istasyon/vagon zeminlerinde kesintisiz NavMesh + kapıda carving | İlk link deneyi A54'te doğrulandı; kullanıcı düzenli ikili giriş ve uzak köprü geometrisini uygun bulmadı. 2026-10-07 revizyonu: kapı vagon kenarında, zeminler 0.05 m aralıklı ve kısa eşikle bağlı; sabit geçiş hatları kaldırılır. Gerçek ayrık geometride link seçeneği ayrıca değerlendirilebilir. |
| T02 | Kapı/zombi/oyuncu için ortak hasar ve sağlık sözleşmesi | M2: sağlık sınırları, tek ölüm/ödül, farklı kaynaklar. |
| T03 | İlk silahlar hitscan/pellet; satın alınmış projectile assetleri VFX | M2/M3: tabanca temeli; M5: çeşitlilik. Yavaş projectile gelecekte ayrı resolver kullanır. |
| T04 | Küçük enemy durum makinesi, veri tabanlı tür farklılıkları | M3/M6: türler aynı döngüyü kullanır; gereksiz framework eklenmez. |
| T05 | Spawn canlı sınırları, havuzlar ve dağıtılmış AI/path güncellemeleri | M6/M9: cihazda ölçüm; sayısal bütçeler profil sonucuyla belirlenir. |
| T06 | D31 ile oyuncunun konumuna turret kurulumu; vagon başına havuz/kota, otomatik para ödülü | Sabit pad önerisi kullanıcı geri bildirimiyle kaldırıldı. Fiziksel para toplama ileride eklenebilir. |
| T07 | Rastgele atamada eşit vagon olasılığı; aynı vagon tekrar çıkabilir | M4: tasarım denemesi. Kullanıcı henüz bu dağılımı seçmedi. |
| T08 | Ölümde oyun biter; kararma/onarım/satın alma eski koşuyu yeniden başlatamaz | M2/M4: ölüm her evrede geçerli. |
| T09 | M1'de kaba Android kalabalık ölçümü, maliyetlerin ayrılması | Ölçüm olmadan NavMesh değiştirilmez veya mobil performans kanıtlandı denmez. |
| T10 | Minimum mobil kontrol/tabanca M3'e alınır; kullanıcıyla oynanış değerlendirmesi | M3: hareket–ateş–onarım deneyimi; M5: kontrol iyileştirme/silah çeşitliliği. |
| T11 | M4a iki vagonla döngü; M4b beş vagona genişletme | Önce atama/kalıcılık/yolculuk kanıtlanır, sonra geometri ve yük genişler. |
| T12 | Botlu yarış nick/karar/hareketle canlı hissi verir; bilgisayar rakipleri olduğu anlaşılır | Gerçek çevrimiçi insan eşleşmesi diye sunmak güven beklentisini bozabilir. Bu açıklık teknik öneridir; kullanıcı tarafından ayrıca seçilmiş sayılmaz. |
| T13 | Hasarlı sağlam kapı da 3 saniyelik yakınlık onarımıyla tam cana döner | 2026-10-08: kullanıcı bu eksik durumların ele alınmasını istedi. Hasar kaynaklarının kesilme kuralları D20 ile ayrı netleştirildi. Yarım süre can vermez; sağlam kapıda yaşam kimliği korunur, kırık kapı açıkça yeniden kurulur. |
| T14 | D02 görsel hareketi istasyon zemini/çevresini de kapsar; istasyon traverslerle döngülenmez | 2026-10-08 kullanıcı geri bildirimi: yalnız ray hareketi yetersizdi. Sabit istasyon colliderının görünümü ayrı köke taşınır; duruşta tam hizalanır. Yol çevresi sınırlı döngü, istasyon tek yaklaşma/kalkış hareketi kullanır; yeni istasyon tam karanlıkta yerleştirilir. Spawn/nav bu görsellere bağlı değildir. |

## Uygulama öncesi veya ilgili aşamada netleştirilecekler

Bu liste ilk navigasyon/hasar denemesini engellemez. İlgili özellik başlamadan karar verilir; yanıt alınmamış tercih kesinleşmiş gibi uygulanmaz.

| Konu | İlk öneri / açık detay | Gerektiği aşama |
| --- | --- | --- |
| Hedef Android cihazları | En az bir düşük ve bir orta sınıf gerçek cihaz seçmek; ölçüm olmadan sabit FPS garantisi yok. | M1 ölçüm düzeni, M6 ilk kalabalık bütçesi |
| Vagonlar arası geçiş | D16: oyuncu atandığı vagonda kalır, kırık kapıdan istasyona çıkamaz. Gelecekte görev bazlı inme izni açıkça verilir. Zombilerin başka vagona gidip gitmeyeceği ayrı. | M3/M4 |
| Onarımda kapanış ve saldırı | Geçitte karakter varsa kapanış bekler; ateş edilebilir. D20: varsayılan oyuncu hasarında kesilmez, kapı hasarında sıfırlanır; iki kural ayrı anahtarlardır. Sayısal denge oynanış değerlendirmesine açık. | M3 devamı / denge |
| Doğuş güvenliği | D28: geçerli adaylardan en yüksek düşman mesafesi + ayarlanabilir 2 sn hasar koruması kullanıcı tarafından seçildi; içerideki zombiler korunur. Gerçek oynanış dengesi açık. | M7b öncesi / denge |
| Kalkış uyarısı | İlk öneri 60 saniye savunma sonrasında 3 saniye uyarı; fade/yavaşlama süreleri ayrı ayarlar. | M4 |
| Düşman hedef önceliği | Atanmış vagonda oyuncu, turret veya başka hedeflerin önceliği; boş vagondaki zombi davranışı. | M3/M7 |
| Savunmaların canı ve patlaması | D29: turret hedef alınmaz; mermi bitişi yakındaki zombilere hasar verir. Dron davranışı M7c öncesi ayrıca seçilir. | M7 |
| Turret sınırının kapsamı | D29: vagon başına her türden en fazla 2; global koşu kotası eklenmedi. | M7 |
| Mobil kontrol hissi | Şema D13 ile seçildi; sol joystick ve otomatik ateşin ergonomisi kullanıcı/cihaz denemesinde değerlendirilir. | M3 değerlendirmesi |
| Silah çeşitlerinin mermisi | D19 sınırlı yedek; D26 ilk alım açar, tekrar alım eksik mermiyi aynı fiyatla tamamlar. Sayısal denge açık. | M7/M8 |
| Mağaza erişimi | D25 ile savaşta açık, oyun sürer; karanlık/ölümde kapalı. Mobil ergonomi ayrıca değerlendirilecek. | M7 değerlendirmesi |
| Para toplama | Kullanıcı para toplama istedi; ilk öneri ödülü otomatik cüzdana yazmak. Görsel pickup/yerden toplama tercihi açık. | M7 |
| Koşu devamı / metagelişim | Hangi kazanımların kaldığı ve devam beklentisi erken kâğıt üzerinde seçilir; disk kaydı ihtiyaç kadar uygulanır. | M4a öncesi tasarım, M8 uygulama |
| Ödüllü reklam / IAP'nin ekonomi rolü | Kullanıcının Battle sonuç 3×, hikâye devam/3×, reklam kaldırma ve yardım teklifi fikirleri alındı. Kaynaklı öneriler MONETIZATION.md içinde; kalıcı ödül, ürün, fiyat ve tekrar sınırı henüz seçilmedi. | M7/M8 tasarım, M9 entegrasyon |

## 2026-10-08 — D24: yan pencerelerden atış

İki taraftaki yan pencerelerden ateş edilir; metal çerçeve/dikme/alt-üst paneller mermiyi durdurur. Kullanıcının D23 sonrasındaki açık isteğiyle önceki dekoratif/opak pencere davranışı değiştirilir. Pencereler zombi veya oyuncu geçişi değildir; cam kırılması/onarımı eklenmez. Oyuncu ve ileride savunmalar ortak raycast geometrisini kullanır. Modelcinin pivot, ölçek, net cam ve metal hacimleri [MODEL_CONTRACT.md](MODEL_CONTRACT.md) ve üretilen ölçü CSV'sinde kayıtlıdır. Doğrulama: dört silahla tüm pencereler, metal kenarlar/dikmeler, eğik raylar ve pencere üzerinden beden/nav geçişinin kapalı olması; mevcut istasyon/kalıcılık kabulü de tekrar çalıştırılır.

## Bilinen tasarım riskleri

- M7a'da boş vagonlara da yeni üretim yapılması kullanıcı oyununda ani ölüm yarattı; D27 uygunluğu ve D28 doğuş koruması M7b öncesi ele alınır. Botlar bu temel kurala bağımlıdır, sorunun geçici çözümü sayılmaz.
- Battle sonucu ödül 3× ve hikâyede reklamla devam/3× kullanıcının hedef fikirleridir; tekrar sınırı/kalıcı ödül/fiyat netleşmedi. D30 kullanıcı tercihi reddin ardından kısa sonuç geçiş reklamıdır; bağımsız yerleşim eski teknik öneridir, seçilmiş karar değildir. Araştırma/alternatifler MONETIZATION.md içinde.

- Kalıcı kapı hasarı ve rastgele vagon ataması, oyuncuyu çok tehlikeli bir vagona getirebilir. Bu istenen risk/şans hissini destekler; anında kaçınılmaz ölüm olmaması için doğuş güvenliği gerekir.
- Turretler eski vagonlarda kaldığından oyuncu dışında öldürme/ödül devam edebilir. Karanlıkta tüm oynanış durur; görünür zamanda bu etki ekonomi dengesiyle değerlendirilir.
- Süreli kalkışta içeride kalan zombi sayısı her istasyonda artabilir. Global canlı sınırı sadece yeni spawnı değil taşınan zombileri de saymalıdır; içeri girmiş zombi sırf bütçe doldu diye yok edilmez.
- Kapıyı onarıp tekrar kırdırmanın ücretsiz onarım döngüsü, güvenli bir sonsuz savunmaya dönüşebilir. Onarım süresi, menzil, kapanış güvenliği ve saldırı temposu birlikte oynanarak dengelenir.

## Karar değişikliği biçimi

Yeni kayıt: tarih, ilgili kimlik, önceki karar, yeni karar, değişiklik nedeni, etkilenen aşamalar/dosyalar ve yeniden yapılacak kontrol. Teknik öneri başarılı deneyden sonra doğrulandı olarak işaretlenir; oyuncu tasarımı ayrıca kullanıcıyla netleştirilir.

## 2026-10-07 — Dış inceleme sonrası plan revizyonu

Kullanıcı mantıklı bulunan önerilerin belgelere işlenmesini istedi. Önceki M3 masaüstünde, mobil kontrol M5'te; M4 beş vagonlu tek aşamaydı. Yeni sıra T09–T11 ile erken cihaz ölçümü, mobil oynanış değerlendirmesi ve iki vagonlu ilk döngüyü içerir. `ROADMAP.md` yeniden kullanım tablosu, kapsam daraltma seçenekleri ve gerçek çalışma hızından sonra süre tahmini yaklaşımıyla güncellendi.

Eklenenler: kontrol/mermi, koşu devamı/metagelişim ve reklam/IAP ekonomi tasarımının erken konuşulması; riskle orantılı otomatik/elle test ayrımı. Korunanlar: NavMesh başlangıç tercihi, ortak hasar/yaşam döngüsü kuralları, pooling eklendiği anda temizlik kontrolü ve dronun oyun kimliğindeki önemi. Waypoint'e erken geçiş, gerekli yaşam döngüsü doğrulamalarını erteleme ve dronu otomatik yayın kapsamından çıkarma alınmadı.

Etkilenen belgeler: ROADMAP, DECISIONS, ARCHITECTURE, GAME_DESIGN ve geliştirme kaydı. Kod değişmedi; yeni kabul koşulları henüz denenmedi ve hiçbir geliştirme aşaması tamamlandı işaretlenmedi.

## 2026-10-08 — M6 teknik uygulama kaydı

StationSpawner + saf SpawnSchedule mevcut Run katmanında, EnemyProfile mevcut Enemies katmanında kuruldu. Yeni doğuşlar bütün vagonlardaki taşınan bedenleri sayar; dolu vagon istekleri kuyruk yerini serbest bırakıp kısa ertelenir. Geçersiz konum en fazla üç denemeden sonra iptal edilir; station bitişi bütün bekleyen üretimi kapatır. Havuz ısınması ve aktive etme ayrı kare bütçeleriyle yapılır. Bu teknik tercih ve başlangıç sayıları nihai denge kararı değildir. Normal/hızlı/dayanıklı test değerleri, 1/3/6+ programları ve açık Android kontrolü SPAWNING.md içinde. Kullanıcının mevcut iki taraflı/4–6 kapılı tren, camdan atış, iç zombi kalıcılığı ve rutin PC kontrolü kararları korunur.

## 2026-10-08 — Kullanıcı düzeltmesi: uçlar metal, daha dar cam, daha yavaş zombi

D24 güncellemesi: her vagonun iki yanındaki dört uç bölüm penceresiz, tamamen metal olur; yalnız dış kapılar arasında yan cam bulunur. Dış pencere çerçevesi 0,32 m, ara dikme 0,18 m; net açıklıklar MODEL_CONTRACT.md ve sahneden üretilen ölçü listesinde kayıtlıdır. Hareket/NavMesh sınırı ve kapı camı kuralları değişmez. Zombi hareket hızları normal 2,6 → 2,1, hızlı 3,8 → 3,0, dayanıklı 1,8 → 1,45 m/sn oldu; can, vuruş, saldırı temposu ve doğuş programı değişmedi. Hızlar Inspector'dan ayarlanabilir başlangıç dengesi olarak kalır.

## 2026-10-08 — D29: sabit turret kapsamı

Kullanıcı seçimi: normal ve gelişmiş turret için **vagon başına her türden 2** sınırı; şimdilik düşmanlar yalnız oyuncu/kapılara saldırır. Mermi bitiş patlaması yakındaki zombilere hasar verir. Dron kamikazesi ayrı M7c işidir. Teknik uygulama: mevcut ortak atış/metal-cam geometrisi, tek alan hasarı ve yakalanmış sahip yaşamıyla ödül; kurulum aktörleri yeniden kullanılır. Sayısal değerler ve PC kabulü [DEFENSES.md](DEFENSES.md) içinde. Turret ölüm/yeniden başlangıç ve vagon kalıcılığı aynı Run akışına bağlanır.

## 2026-10-08 — D30: gelir planının ertelenmesi ve sağlayıcı tercihi

Kullanıcı odağı önce oyun olarak seçti; IAP ekonomisi/ürün/fiyat henüz kararlaştırılmadı. İlk yayın Google Play, IAP teslimi onun faturalandırmasıyla hedeflenir. Reklam için Unity Ads tercih edilir; bu sağlayıcının gelir üstünlüğü doğrulanmış sayılmaz. Kullanıcı Battle 3×, hikâye devam/3× tekliflerini ve teklif reddinden sonra kısa sonuç geçiş reklamı fikrini yeniden belirtti. Önceki bağımsız reklam yerleşimi önerisi kullanıcı tarafından seçilmiş karar değildir; yeni tercih esas alınır. Gösterim sıklığı, reklam kaldırma hakkı, reklam yok/hata akışı ve güncel platform kuralları M9 somut servis tasarımında doğrulanır. Bu aşamada SDK eklenmedi.

## 2026-10-08 — D31: serbest turret kurulumu ve tren dizilimi

Kullanıcı sabit pad yaklaşımını kaldırdı: kendi vagonunda uygun zeminde satın alınan turret oyuncunun o andaki X/Z konumuna kurulur. Her türden 2/vagon kotası kalır. Dört yeniden kullanılabilir aktör/vagon; zombi/metal/başka turret üstüne kurulum ücret almadan reddedilir. Görünüm değişmeden gövde 0,22 × 0,6 × 0,22 m, carving 0,24 × 0,7 × 0,24 m; sahibi içinden yürür. Atama sonrası CharacterController yeniden etkinleştirildiğinde bu çarpışma hariç tutması yenilenir. Turret kurulmuş konum/mermiyle vagonda kalır.

D24 görünüm düzeltmesi: pencere üst kotu 1,9, üst metal 0,2 m; dikey dış çerçeve 0,42 ve ara dikme 0,32 m. Bütün görünür duvarlar 2,1 m üst kotta birleşir. Kapı üst çerçevesi gövdede sabittir, kapıyla gizlenmez. Cam/metal fiziği bu gerçek hacimlerle uyumludur; köşeler penceresiz kalır.

Tren +X yönüne gider; kuyruktan başa kapı/can dizilimi 6/10 → 5/20 → 5/26 → 4/32 → 4/40. En sağdaki güçlü vagonun sağında kapalı lokomotif/kontrol odası vardır; orası hedef, spawn alanı veya oynanabilir vagon değildir. Botlar henüz eklenmedi; M7c sonrası Mod/Bot sırası korunur.

Normal koşu her Restart'ta yeni rastgele tohum ve rastgele ilk vagon kullanır. Önceki 12345 sabit tohumu her oyunda aynı diziyi üretiyordu. Sabit tohum testlerde açık seçenek olarak korunur; fixture sonunda normal ayar geri yüklenir. Aynı vagonun tesadüfen tekrar seçilmesi mümkündür; sıra garantisi yoktur. ActualRunSeed ileride kayıtla RNG devamını çözmek için tek başına yeterli değildir; M8 mevcut RNG konumunu da korumalıdır.

Deneme kolaylığı için başlangıç parası RunShop.asset üzerinden geçici 1000 yapıldı; yayın dengesi kabulü değildir. Güncel model ölçüleri MODEL_CONTRACT ve üretilen CSV'lerdedir. Bu düzeltmelerden sonra sonraki mekanik M7c drondur.

## 2026-10-08 — D32: dron kamikazesi ve engeller

Kullanıcı onayı: mermisi biten dron erişebildiği en yakın zombiye gider; kapalı kapı/metalden geçmez, uygun hedef bulamazsa kısa süre arayıp kapanır. Patlama metalin arkasına hasar vermez. Süre ve değerler ayarlanır. İlk uygulama doğrudan küre taramasıyla açık yolu seçer; duvar üstünden geçmez, NavMesh veya karmaşık rota araması eklemez. Cam mermi geçirir ama fiziksel uçuşu engeller. Karanlıkta süre durur; kayıp hedef/atama süreyi sıfırlamaz. Oyuncuda bir dron, oyuncuyla kalıcılık, ortak atış/ödül ve yeniden kullanılan tek aktör. Ayrıntılar DRONE.md.

## 2026-10-08 — D33: Battle elemesi ve sonuç

Kullanıcı soruda onayladı: insan ölünce sırası gösterilip koşu biter; bütün botlar elenip yalnız insan kalınca birincilikle biter. Aynı karede elenenler aynı sırayı paylaşır. Teknik uygulama ölüm bayraklarını kare sonunda toplar; son iki katılımcı birlikte ölürse yaşayan kazanan ilan etmez, ortak sıra ve beraberlik gösterir. Ölü katılımcı aynı koşuda yeniden doğmaz. Yeni atama tüm yaşayan katılımcıları çakışmadan dağıtır; boş vagonlar yeni atamada seçilebilir. Yerel botlu Battle ile botsuz ilerleme modu ortak RunFlow kullanır. BOTS.md uygulama/sınır/ayar kaydıdır; gerçek multiplayer, sonuç reklamı veya kalıcı ödül eklenmedi.

## 2026-10-09 — D34: hareket yönü ve M8a kayıt temeli

Kullanıcı talebi: insan ve bot yürürken hareket yönüne baksın; geçerli ateş hedefi varsa yüzünü ona dönsün. Ortak motor bunu uygular; görsel model değişimi kuralı değiştirmez.

Önceden seçilen uygulama kapanınca mevcut koşuya devam / ölümde yeni koşu hedefi M8a yerel kayıtla uygulanır. Battle elemesi, vagonların ayrı durumu ve RNG ilerlemesi birlikte korunur. Mühendislik başlangıç ayarı beş saniyelik periyodik kayıt ve mobil yaşam döngüsü callback'leridir; Inspector'dan değişir. Editor test izolasyonu için otomatik devam varsayılan kapalı, F5/F6 açıktır. Bulut, kalıcı ödül/IAP ve kullanıcı menüsü bu adımda eklenmedi; Android süreç kapatma kabulü ayrı kalır. Sözleşme ve sınırlar PERSISTENCE.md içindedir.

## 2026-10-09 — D35: kalabalıkta vagon sınırı ve yumuşak yürüyüş dönüşü

Kullanıcı açık kapıda zombi baskısının oyuncuyu dışarı ittiğini ve WASD yürüyüş dönüşlerinin keskin olduğunu bildirdi. Ortak motor çarpışma sonrası gerçek kapsülü de MovementArea içinde tutar. İnsan ve bot, hedef yokken yürüyüş girdisi yönüne ayarlanabilir 720°/sn ile döner; nişan önceliği/hızlı dönüş korunur. Fiziksel itme boşta yön seçmez. Kapı/zombi navigasyonu ve model ölçüleri değişmez. Aynı kapı-kalabalık testi eski kodda başarısız, yeni kodda başarılıdır; uygulama ve kabul kaydı PLAYER_MOVEMENT.md içindedir. Bu bir M8a sonrası hata düzeltmesidir; sıradaki aşama M8b kalır.

## 2026-10-09 — D36: iki mod ve yayın doğrulama yolu

Kullanıcı ilk yayında Battle ve Solo'yu **eşit önemde** seçti; bir mod otomatik ertelenmez. Kullanıcı kamera/hasar/patlama hissi, cihaz akıcılığı ve ticari başarıyı önceliklendirdi; IAP ürünlerini kesinleştirmedi. Mühendislik önerisi: mevcut A54 ölçümü → hafif geri bildirim/akış → iki modda oynanış değerlendirmesi → tek vagon sanat örneği → bütçeli içerik/gelir/sınırlı yayın. Bu sıranın kabul koşulları RELEASE_PLAN.md içinde; kamera efektleri, telemetri, reklam/IAP ve yeni cihaz kabulü henüz uygulanmış değildir. Her telefonda sıfır düşüş veya garantili gelir/mağazada öne çıkarılma iddiası yapılmaz; cihaz bütçesi ve gerçek kullanıcı/gelir ölçümüyle ilerlenir.

Battle süre açıklaması: kullanıcı 5–8 dakika yanıtını hemen ardından kesinleştirmediğini belirtti; yaşayanların devam edebilmesini istedi. Zorunlu toplam yarış sayacı eklenmez; D33 sürer. 5–8 dakika yalnız ilk tempo değerlendirmesi için geçici referans, garanti veya bitiş kuralı değildir.

## 2026-10-09 — D37: Solo fiziksel ilerleme ve iki modda kalıcı güç

Kullanıcının güncel hedefi: Solo kuyruktaki en zayıf vagonda başlar; koşu parasıyla sıradaki daha güçlü vagon açılır, iç uç bağlantılarından yürünür. Açılmış vagonlar arasında geri geçiş serbest; kullanıcı soruda **yeni saldırılar bulunduğum vagona, mevcut iç zombiler korunsun** seçti. Solo'da rastgele atama kaldırılacak; Battle rastgele atama/eleme korunur. Yeni vagon bir defalık yardım/ganimet sağlayabilir; kesin loot listesi, iç bağlantı kapısı etkileşimi ve bedeller açık.

İki toplam para kaynağı hedefi: koşuda kazanılan/harcanan para ve sonuçtan sınırlı kazanılan/IAP ile satılabilen kalıcı para. Kalıcı silah açılışı sonrası koşu içi satın alma; küçük can/menzil/hız geliştirmeleri. Kullanıcı soruda **kalıcı güç iki modda da çalışsın** seçti; Battle güç normalizasyonu mühendislik önerisi seçilmedi. Ad/fiyat/üst sınır, ödül formülü ve paketler henüz seçilmedi. D30 Unity Ads/Google Play reklam akışı korunur. Solo ilk öğretici, sonra Battle açılışı hedef; koşul açık.

Güncel tasarım PROGRESSION_DESIGN.md, sıra RELEASE_PLAN.md. M8b mevcut cihaz temeli ardından M8s iki-vagon Solo, M8e minimum meta ve M8c kullanıcı akışı; M9a sanat örneği geçit ölçüsünden sonra öne alınabilir. Mevcut v1 kayıt ve kapalı uç geometrisi yeni Solo kabulü değildir; yeni sürüm/geçit ölçüsü gerekir. Bugünkü kod yeni ilerleme/kalıcı ekonomi veya yeni bot sunumunu henüz uygulamaz. Doğal bot davranışı/nickler kullanıcı hedefidir; gerçek online eşleşme iddiası yerine açık yerel yarış sunumu mühendislik önerisidir, kullanıcı tarafından kesinleşmiş metin değildir.

## 2026-10-09 — D38: yatay mobil ekran ve ölçüm kapsamı

Kullanıcı portre açılışın yanlış olduğunu ve joystick dokunuşlarının siyah kenarlarda `Move` izleri bıraktığını bildirdi. Yalnız iki yatay yön açık kalır. Güvenli alan dışındaki arka tampon her karede temizlenir; joystick görseli güvenli alanda tutulur, giriş başlangıcı yer değiştirmez. Kamera/framing, collider, atış ve hasar sözleşmeleri korunur. Mobilde açık 60 FPS hedefi ve seçilebilir 30 FPS API'si uygulanır; bu bütün cihazlarda 60 garantisi değildir.

Kullanıcı bu turda **kablolu sıcaklık/FPS testi** seçti. USB güç varken pil tüketimi sonucu üretilmez. Android Development APK'da yerel teşhis ve isteğe bağlı 10 dakika/60 havuz sınırında sentetik savaş fixture'ı bulunur; yayın build'inden çıkarılır. Normal koşu kaydı ayrı korunur. Ölçüm yöntemi ANDROID_PLAYTEST.md, gerçek A54 sonuçları A54_PERFORMANCE.md içinde. Bu test yeni Solo fiziksel ilerlemesi, son modeller/animasyonlar, üretim UI/reklamlar veya bütün cihazların yayın kabulü değildir. Açılıştaki AssetPackManager sınıf hatası çözülmeden temiz Android yayın kabulü verilmez.
