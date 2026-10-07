# M2 — ortak can/hasar temeli

`CombatSandbox.unity` yalnızca masaüstü doğrulama sahnesidir. `SampleScene 2` ve eski silah/enemy scriptleri değiştirilmez; üretim entegrasyonu M3'tedir.

## Modül sınırı

- `WaitYourTurn.Combat`: Unity referansı olmayan saf `Health`, hasar sözleşmeleri ve kurallar. Navigasyon, mağaza, silah, UI veya ödül servisi bilmez. Update/metot başına allocation yoktur.
- `WaitYourTurn.Combat.Unity`: `HealthComponent` Unity adaptörü; can değişimi ve tek ölüm bildirimi yayımlar. Aynı component oyuncu/zombi/kapıda kullanılabilir. Runtime nesne kimliği Unity 6.6 `EntityId.ToULong(GetEntityId())` ile alınır; can çekirdeğine Unity tipi taşınmaz.
- `Sandbox`: geçici fare girdisi, 2 hasarlı hitscan denemesi ve gözlem düğmeleri. Mobil kontrol/tabanca şarjör kuralı veya üretim WeaponController'ı değildir.
- `Tests/EditMode`: saf can kuralları ve ölüm bildirim sınırı. Test assembly'si oyuncu buildine girmez.

## Kurallar

Hasar/heal/max can yalnızca pozitif, sonlu sayıları kabul eder. `0 ≤ Current ≤ Maximum`; overkill sonucu verilen gerçek hasardır. Ölü hedef tekrar ölüm üretmez, normal/shop heal ile dirilmez. Yeni yaşam yalnızca açık `ResetForSpawn` çağrısıyla başlatılır; yaşam sürümü artar ve önceki hedef yaşamına ait vuruş reddedilir. Bu, havuzdan dönen aynı GameObject'in yeni yaşamını eski vuruştan ayırır.

Shop heal: `min(missingHealth, max(30, currentHealth × 0.5))`. 10/100 → 40/100; 50/100 → 80/100; 80/100 → 100/100. Mağaza ücret/işlemi henüz bağlı değildir.

Max can değişiminde varsayılan `PreserveCurrent`: mevcut can korunur, yeni üst sınır daha düşükse kırpılır. `HealAddedCapacity` yalnızca eklenen kapasite kadar can verir; ölü hedefi diriltmez. Ürün/buff çağıran kod politikayı açıkça seçer.

Takım hasarı varsayılan kapalıdır; nötr kapı çevre hedefidir. Açık friendly-fire izni ve dokunulmazlık ortak kurallardır. Hasar tipi şimdilik bağlamda taşınır; zırh/direnç/bağışıklık hesaplaması uygulanmadı.

`DamageContext` kaynak kimliği/yaşamı, kaynak takım, saldırı kimliği, ödül sahibi ve beklenen hedef yaşamını taşır. Saldırı kimliği kayıt/atfetme içindir; burada sınırsız isabet geçmişi tutulmaz. Patlamanın collider başına çift isabetini HitResolver tek hedefe indirger; pompalının ayrı saçmaları ayrı isabet olabilir. Kaynak yaşamının/gecikmiş animasyon callback'inin geçerliliği saldırıyı başlatan sistemde M3/M5'te denetlenir.

`Died(DeathNotice)` oyuncu ölümünde koşu sonu, zombi ölümünde ödül tüketicisine açık sinyaldir. Para/koşu yöneticisi henüz yazılmadı; lab sinyali sayar. Bildirim sırasında yeniden hasar/reset girişimi reddedilir; pool spawn işlemi ölüm dinleyicisinin içinden aynı çağrı zincirinde yeniden başlatılmaz. Despawn ve öldürme aynı işlem değildir.

## Çalıştırma ve kanıt

Unity menüsü **Wait Your Turn → Combat → Run Health Tests**. Kurulu Unity Test Framework **1.8.0** kullanılır; paket yükseltilmedi. Sonuç `Logs/HealthTests.xml` ve Console'a yazılır. 2026-10-07 UTC 17:07:36 sonucu: **14 passed / 0 failed**, yaklaşık 0.065 saniye. Kapı 8+2, negatif/NaN/infinity, heal sınırı, max can politikası, overkill, takım/dokunulmazlık, eski yaşam vuruşu ve tek ölüm bildirimi kontrol edildi.

**Wait Your Turn → Combat → Open Sandbox**, sonra Play. Beyaz zombi/kırmızı kapıya kontrol panelinin dışında tıklamak test tabancasını ateşler; gerçek raycast isabeti ortak can sistemine 2 hasar verir. `Door -8/-2`, `Player -> 10 HP`, `Shop heal`, `Max HP +25`, `Kill player`, `Reset test lives` düğmeleri kuralları gözlemletir. Düğmeler lab hasarıdır; enemy AI değildir. Yeni yaşam başlatma düğmesi kapı onarımı değildir.

2026-10-07 Editor Play kontrolü: fareyle beyaz hedefe isabet **10 → 8**, kapı düğmeleri **10 → 2 → 0**, oyuncu **100 → 10 → shop heal ile 40**, ölüm **0 can / Run ended: True** olarak görüldü. Bu kontrol sırasında yeni exception oluşmadı. Sahne kaydedildi ve Play durduruldu.

Kapı canı ile nav portalının kırılma/onarım ilişkisi, enemy saldırı aralığı, pool, mobil kontrol ve gerçek oyuncu/efekt bağlantıları **M3** işidir. Bu belge onları uygulanmış saymaz.
